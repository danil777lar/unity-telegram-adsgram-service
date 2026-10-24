using UnityEngine;

namespace Larje.Ads.Telegram
{
    [CreateAssetMenu(fileName = "AdsgramAdsConfig", menuName = "Larje/Ads/Adsgram Ads Config")]
    public sealed class AdsgramAdsConfig : ScriptableObject
    {
        [Header("Blocks")]
        [SerializeField] private string interstitialBlockId;
        [SerializeField] private string rewardedBlockId;

        [Header("Interstitial")]
        [SerializeField, Min(0f)] private float interstitialCooldown = 30f;

        [Header("System")]
        [SerializeField] private bool debug;

        public string InterstitialBlockId => interstitialBlockId;
        public string RewardedBlockId => rewardedBlockId;
        public float InterstitialCooldown => interstitialCooldown;
        public bool Debug => debug;
    }
}
