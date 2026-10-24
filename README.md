# Larje Ads Telegram (Adsgram)

`com.larje.ads-telegram`: Adsgram provider for Telegram Mini Apps (WebGL builds) implementing the `IAdsService` from `com.larje.core`: interstitial with a cooldown and rewarded ads as `UniTask<bool>`. A small jslib bridge calls the Adsgram JS SDK and reports results through a native callback.

## Install

UPM cannot resolve git dependencies, so add core, UniTask and this package to `Packages/manifest.json`:

```json
"com.cysharp.unitask": "https://github.com/Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask#2.5.10",
"com.larje.core": "https://github.com/danil777lar/unity-gameplay-core-services.git",
"com.larje.zenject": "https://github.com/danil777lar/unity-zenject-upm.git",
"com.larje.ads-telegram": "https://github.com/danil777lar/unity-telegram-adsgram-service.git"
```

No SDK define is needed. The Zenject installer additionally needs `com.larje.zenject` (`LARJE_ZENJECT`).

### SDK

No Unity package is needed. Add the Adsgram script to the WebGL template `index.html`:

```html
<script src="https://sad.adsgram.ai/js/sad.min.js"></script>
```

Block ids come from the Adsgram partner dashboard (interstitial ids start with `int-`).

## Setup

1. Create `AdsgramAdsConfig` (`Create > Larje > Ads > Adsgram Ads Config`) with the interstitial and rewarded block ids.
2. Create `AdsgramAdsInstaller` (`Create > Larje > Ads > Adsgram Ads Installer`), assign the config and add it to the `ProjectContext` next to `LarjeCoreInstaller`.
3. Build for WebGL and open the build inside Telegram.

## Config

| Group | Fields |
|---|---|
| Blocks | `interstitialBlockId`, `rewardedBlockId`. An empty id disables that format. |
| Interstitial | `interstitialCooldown` seconds after any ad. |
| System | `debug` passes `debug: true` to `Adsgram.init`. |

## Behavior

- Outside WebGL builds (editor, other platforms) no ads are available and `ShowRewardedAsync` returns `false`. Use the simulated service from core for editor testing.
- Rewarded returns `true` when Adsgram resolves `show()` with `done: true`; skips and errors return `false`.
- Banners and app open ads are not supported.
- The previous module passed the request id through `UTF8ToString` and relied on `window.unityInstance.SendMessage`; the bridge now uses `makeDynCall` with a static `MonoPInvokeCallback`.

## Status

Compiles in Unity 6000.4.9f1 (non-WebGL code path). The WebGL bridge is not tested in Telegram yet.
