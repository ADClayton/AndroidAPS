# Omnipod Dash Controller (MAUI)

This proof-of-concept .NET MAUI shell mirrors the Omnipod Dash control surface from the AndroidAPS
`pump/omnipod-dash` module. The goal is to allow a MAUI host to attach to a pod that was
already activated by AndroidAPS while reusing the same command names and activation order
exposed by `OmnipodDashManager`.

## Extracted protocol hints

* The Android driver uses a fixed nonce of `1229869870` when negotiating the session
  (`OmnipodDashManagerImpl.NONCE`).
* Core commands are aligned with the `OmnipodDashManager` interface: connect, get status,
  program basal delivery, temp basal, bolus, alerts, beeps, and suspend/deactivate flows.
* Connection events follow the `PodEvent` hierarchy (`AlreadyConnected`, `Pairing`,
  `BluetoothConnected`, `CommandSending`, `ResponseReceived`, etc.).

## Current status

The MAUI app provides a simple UI with the pod ID, optional pairing PIN, and a log of
pod events. Platform-specific BLE handling still needs to be implemented; the
`OmnipodDashClient` class exposes the right hooks so Android/iOS implementations can be
plugged in while preserving the ordering and terminology from the Kotlin driver.
