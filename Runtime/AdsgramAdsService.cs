using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using AOT;
using Cysharp.Threading.Tasks;
using Larje.Core;
using Larje.Core.Monetization;
using Larje.Core.Saves;
using UnityEngine;

namespace Larje.Ads.Telegram
{
    public sealed class AdsgramAdsService : AdsServiceBase, IInitializableService, IDisposable
    {
        private const string INTERSTITIAL_KEY = "interstitial";
        private const string REWARDED_KEY = "rewarded";
        private const int STATUS_DONE = 1;

        private static readonly Dictionary<int, UniTaskCompletionSource<int>> s_requests = new Dictionary<int, UniTaskCompletionSource<int>>();
        private static int s_nextRequestId;

        private bool _initialized;
        private bool _interstitialReady;
        private bool _rewardedReady;
        private bool _fullscreenShowing;
        private float _lastFullscreenTime = float.MinValue;
        private readonly AdsgramAdsConfig _config;

        public override bool Initialized => _initialized;
        public override bool InterstitialAvailable => _interstitialReady && !_fullscreenShowing && CooldownPassed;
        public override bool RewardedAvailable => _rewardedReady && !_fullscreenShowing;
        public override bool BannerShowing => false;
        public override float BannerHeight => 0f;

        private bool CooldownPassed => Time.realtimeSinceStartup - _lastFullscreenTime >= _config.InterstitialCooldown;

        public AdsgramAdsService(AdsgramAdsConfig config, ISaveService saves) : base(saves)
        {
            _config = config;
        }

#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern int LarjeAdsgramInit(string key, string blockId, int debug);

        [DllImport("__Internal")]
        private static extern void LarjeAdsgramShow(string key, int requestId, Action<int, int> callback);
#endif

        public void Initialize()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            int debug = _config.Debug ? 1 : 0;
            if (!string.IsNullOrEmpty(_config.InterstitialBlockId))
            {
                _interstitialReady = LarjeAdsgramInit(INTERSTITIAL_KEY, _config.InterstitialBlockId, debug) == 1;
            }

            if (!string.IsNullOrEmpty(_config.RewardedBlockId))
            {
                _rewardedReady = LarjeAdsgramInit(REWARDED_KEY, _config.RewardedBlockId, debug) == 1;
            }
#else
            Debug.Log("AdsgramAdsService: Adsgram works only in WebGL builds inside Telegram");
#endif
            _initialized = true;
        }

        public void Dispose()
        {
            foreach (UniTaskCompletionSource<int> request in s_requests.Values)
            {
                request.TrySetResult(0);
            }

            s_requests.Clear();
        }

        protected override void ShowInterstitialInternal(string placement)
        {
            if (!InterstitialAvailable)
            {
                return;
            }

            ShowInterstitialAsync().Forget();
        }

        protected override async UniTask<bool> ShowRewardedInternal(string placement, CancellationToken cancellationToken)
        {
            if (!RewardedAvailable)
            {
                return false;
            }

            _fullscreenShowing = true;
            try
            {
                int status = await ShowBlockAsync(REWARDED_KEY).AttachExternalCancellation(cancellationToken);
                return status == STATUS_DONE;
            }
            finally
            {
                _fullscreenShowing = false;
                _lastFullscreenTime = Time.realtimeSinceStartup;
            }
        }

        protected override bool ShowAppOpenInternal()
        {
            return false;
        }

        protected override void HideBanner()
        {
        }

        private async UniTaskVoid ShowInterstitialAsync()
        {
            _fullscreenShowing = true;
            await ShowBlockAsync(INTERSTITIAL_KEY);
            _fullscreenShowing = false;
            _lastFullscreenTime = Time.realtimeSinceStartup;
        }

        private UniTask<int> ShowBlockAsync(string key)
        {
            int requestId = ++s_nextRequestId;
            UniTaskCompletionSource<int> completion = new UniTaskCompletionSource<int>();
            s_requests[requestId] = completion;
#if UNITY_WEBGL && !UNITY_EDITOR
            LarjeAdsgramShow(key, requestId, OnShowResult);
#else
            OnShowResult(requestId, 0);
#endif
            return completion.Task;
        }

        [MonoPInvokeCallback(typeof(Action<int, int>))]
        private static void OnShowResult(int requestId, int status)
        {
            if (s_requests.TryGetValue(requestId, out UniTaskCompletionSource<int> completion))
            {
                s_requests.Remove(requestId);
                completion.TrySetResult(status);
            }
        }
    }
}
