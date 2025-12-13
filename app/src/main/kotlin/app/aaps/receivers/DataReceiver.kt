package app.aaps.receivers

import android.content.Context
import android.content.Intent
import android.provider.Telephony
import androidx.work.Data
import androidx.work.OneTimeWorkRequest
import app.aaps.core.interfaces.logging.AAPSLogger
import app.aaps.core.interfaces.logging.LTag
import app.aaps.core.interfaces.receivers.Intents
import app.aaps.core.interfaces.utils.fabric.FabricPrivacy
import app.aaps.core.utils.extensions.copyDouble
import app.aaps.core.utils.extensions.copyLong
import app.aaps.core.utils.extensions.copyString
import app.aaps.core.utils.receivers.BundleLogger
import app.aaps.core.utils.receivers.DataWorkerStorage
import app.aaps.plugins.main.general.smsCommunicator.SmsCommunicatorPlugin
import app.aaps.plugins.source.DexcomPlugin
import app.aaps.plugins.source.GlimpPlugin
import app.aaps.plugins.source.MM640gPlugin
import app.aaps.plugins.source.OttaiPlugin
import app.aaps.plugins.source.PoctechPlugin
import app.aaps.plugins.source.SyaiTagPlugin
import app.aaps.plugins.source.TomatoPlugin
import app.aaps.plugins.source.XdripSourcePlugin
import dagger.android.DaggerBroadcastReceiver
import javax.inject.Inject

open class DataReceiver : DaggerBroadcastReceiver() {

    @Inject lateinit var aapsLogger: AAPSLogger
    @Inject lateinit var dataWorkerStorage: DataWorkerStorage
    @Inject lateinit var fabricPrivacy: FabricPrivacy

    override fun onReceive(context: Context, intent: Intent) {
        super.onReceive(context, intent)
        val action = intent.action
        val bundle = intent.extras

        if (action == null) {
            aapsLogger.error(LTag.CORE, "DataReceiver.onReceive called without action. Verify the broadcast configuration and sender.")
            logTriggeringAdvice(context)
            return
        }

        if (bundle == null) {
            aapsLogger.error(LTag.CORE, "DataReceiver.onReceive missing extras for action {}. Ensure the broadcaster includes payload data.", action)
            logTriggeringAdvice(context)
            return
        }

        aapsLogger.debug(LTag.CORE, "onReceive {} {}", action, BundleLogger.log(bundle))

        val request = when (action) {
            Intents.ACTION_NEW_BG_ESTIMATE            ->
                OneTimeWorkRequest.Builder(XdripSourcePlugin.XdripSourceWorker::class.java)
                    .setInputData(dataWorkerStorage.storeInputData(bundle, intent.action)).build()

            Intents.POCTECH_BG                        ->
                OneTimeWorkRequest.Builder(PoctechPlugin.PoctechWorker::class.java)
                    .setInputData(Data.Builder().also {
                        it.copyString("data", bundle)
                    }.build()).build()

            Intents.GLIMP_BG                          ->
                OneTimeWorkRequest.Builder(GlimpPlugin.GlimpWorker::class.java)
                    .setInputData(Data.Builder().also {
                        it.copyDouble("mySGV", bundle)
                        it.copyString("myTrend", bundle)
                        it.copyLong("myTimestamp", bundle)
                    }.build()).build()

            Intents.TOMATO_BG                         ->
                @Suppress("SpellCheckingInspection")
                OneTimeWorkRequest.Builder(TomatoPlugin.TomatoWorker::class.java)
                    .setInputData(Data.Builder().also {
                        it.copyDouble("com.fanqies.tomatofn.Extras.BgEstimate", bundle)
                        it.copyLong("com.fanqies.tomatofn.Extras.Time", bundle)
                    }.build()).build()

            Intents.NS_EMULATOR                       ->
                OneTimeWorkRequest.Builder(MM640gPlugin.MM640gWorker::class.java)
                    .setInputData(Data.Builder().also {
                        it.copyString("collection", bundle)
                        it.copyString("data", bundle)
                    }.build()).build()

            Intents.OTTAI_APP                       ->
                OneTimeWorkRequest.Builder(OttaiPlugin.OttaiWorker::class.java)
                    .setInputData(Data.Builder().also {
                        it.copyString("collection", bundle)
                        it.copyString("data", bundle)
                    }.build()).build()

            Intents.SYAI_TAG_APP                       ->
                OneTimeWorkRequest.Builder(SyaiTagPlugin.SyaiTagWorker::class.java)
                    .setInputData(Data.Builder().also {
                        it.copyString("collection", bundle)
                        it.copyString("data", bundle)
                    }.build()).build()

            Telephony.Sms.Intents.SMS_RECEIVED_ACTION ->
                OneTimeWorkRequest.Builder(SmsCommunicatorPlugin.SmsCommunicatorWorker::class.java)
                    .setInputData(dataWorkerStorage.storeInputData(bundle, intent.action)).build()

            Intents.DEXCOM_BG, Intents.DEXCOM_G7_BG   ->
                OneTimeWorkRequest.Builder(DexcomPlugin.DexcomWorker::class.java)
                    .setInputData(dataWorkerStorage.storeInputData(bundle, intent.action)).build()

            else                                      -> {
                aapsLogger.warn(LTag.CORE, "DataReceiver.onReceive received unsupported action {}. Confirm the intent filter and sender match expected actions.", action)
                logTriggeringAdvice(context)
                null
            }
        }

        if (request != null) {
            dataWorkerStorage.enqueue(request)
            aapsLogger.info(LTag.CORE, "DataReceiver scheduled worker {} for action {}", request.workSpec.workerClassName, action)
        }

        // Verify KeepAlive is running
        // Sometimes the schedule fail
        KeepAliveWorker.scheduleIfNotRunning(context, aapsLogger, fabricPrivacy)
    }

    private fun logTriggeringAdvice(context: Context) {
        aapsLogger.info(
            LTag.CORE,
            "Troubleshooting: if expected broadcasts do not trigger DataReceiver, verify the receiver entry in AndroidManifest.xml is enabled and has android:exported set appropriately, runtime permissions (for example RECEIVE_SMS) are granted, and any third-party sender uses the exact package {} and action names from Intents. Also confirm battery optimizations or vendor-specific power managers are not blocking background receivers.",
            context.packageName
        )
    }

}
