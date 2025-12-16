package app.aaps.di

import app.aaps.core.interfaces.plugin.PluginBase
import app.aaps.plugins.configuration.configBuilder.ConfigBuilderPlugin
import app.aaps.plugins.insulin.InsulinLyumjevPlugin
import app.aaps.plugins.insulin.InsulinOrefFreePeakPlugin
import app.aaps.plugins.insulin.InsulinOrefRapidActingPlugin
import app.aaps.plugins.insulin.InsulinOrefUltraRapidActingPlugin
import app.aaps.plugins.main.general.actions.ActionsPlugin
import app.aaps.plugins.main.general.overview.OverviewPlugin
import app.aaps.plugins.main.general.persistentNotification.PersistentNotificationPlugin
import app.aaps.plugins.main.general.themes.ThemeSwitcherPlugin
import app.aaps.plugins.main.iob.iobCobCalculator.IobCobCalculatorPlugin
import app.aaps.plugins.main.profile.ProfilePlugin
import app.aaps.plugins.smoothing.AvgSmoothingPlugin
import app.aaps.plugins.smoothing.ExponentialSmoothingPlugin
import app.aaps.plugins.smoothing.NoSmoothingPlugin
import app.aaps.plugins.source.XdripSourcePlugin
import app.aaps.pump.omnipod.dash.OmnipodDashPumpPlugin
import dagger.Binds
import dagger.Module
import dagger.multibindings.IntKey
import dagger.multibindings.IntoMap
import javax.inject.Qualifier

@Suppress("unused")
@Module
abstract class PluginsListModule {

    @Binds
    @AllConfigs
    @IntoMap
    @IntKey(0)
    abstract fun bindPersistentNotificationPlugin(plugin: PersistentNotificationPlugin): PluginBase

    @Binds
    @AllConfigs
    @IntoMap
    @IntKey(5)
    abstract fun bindOverviewPlugin(plugin: OverviewPlugin): PluginBase

    @Binds
    @AllConfigs
    @IntoMap
    @IntKey(10)
    abstract fun bindIobCobCalculatorPlugin(plugin: IobCobCalculatorPlugin): PluginBase

    @Binds
    @AllConfigs
    @IntoMap
    @IntKey(20)
    abstract fun bindActionsPlugin(plugin: ActionsPlugin): PluginBase

    @Binds
    @AllConfigs
    @IntoMap
    @IntKey(30)
    abstract fun bindInsulinOrefRapidActingPlugin(plugin: InsulinOrefRapidActingPlugin): PluginBase

    @Binds
    @AllConfigs
    @IntoMap
    @IntKey(40)
    abstract fun bindInsulinOrefUltraRapidActingPlugin(plugin: InsulinOrefUltraRapidActingPlugin): PluginBase

    @Binds
    @AllConfigs
    @IntoMap
    @IntKey(42)
    abstract fun bindInsulinLyumjevPlugin(plugin: InsulinLyumjevPlugin): PluginBase

    @Binds
    @AllConfigs
    @IntoMap
    @IntKey(50)
    abstract fun bindInsulinOrefFreePeakPlugin(plugin: InsulinOrefFreePeakPlugin): PluginBase

    @Binds
    @PumpDriver
    @IntoMap
    @IntKey(148)
    abstract fun bindOmnipodDashPumpPlugin(plugin: OmnipodDashPumpPlugin): PluginBase

    @Binds
    @AllConfigs
    @IntoMap
    @IntKey(240)
    abstract fun bindLocalProfilePlugin(plugin: ProfilePlugin): PluginBase

    @Binds
    @AllConfigs
    @IntoMap
    @IntKey(400)
    abstract fun bindXdripSourcePlugin(plugin: XdripSourcePlugin): PluginBase

    @Binds
    @AllConfigs
    @IntoMap
    @IntKey(490)
    abstract fun bindConfigBuilderPlugin(plugin: ConfigBuilderPlugin): PluginBase

    @Binds
    @AllConfigs
    @IntoMap
    @IntKey(500)
    abstract fun bindThemeSwitcherPlugin(plugin: ThemeSwitcherPlugin): PluginBase

    @Binds
    @AllConfigs
    @IntoMap
    @IntKey(600)
    abstract fun bindNoSmoothingPlugin(plugin: NoSmoothingPlugin): PluginBase

    @Binds
    @AllConfigs
    @IntoMap
    @IntKey(605)
    abstract fun bindExponentialSmoothingPlugin(plugin: ExponentialSmoothingPlugin): PluginBase

    @Binds
    @AllConfigs
    @IntoMap
    @IntKey(610)
    abstract fun bindAvgSmoothingPlugin(plugin: AvgSmoothingPlugin): PluginBase

    @Qualifier
    annotation class AllConfigs

    @Qualifier
    annotation class PumpDriver
}
