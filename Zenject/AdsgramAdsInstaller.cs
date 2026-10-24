using Larje.Core.Zenject;
using UnityEngine;
using Zenject;

namespace Larje.Ads.Telegram.Zenject
{
    [CreateAssetMenu(fileName = "AdsgramAdsInstaller", menuName = "Larje/Ads/Adsgram Ads Installer")]
    public sealed class AdsgramAdsInstaller : ScriptableObjectInstaller<AdsgramAdsInstaller>
    {
        [SerializeField] private AdsgramAdsConfig config;

        public override void InstallBindings()
        {
            Container.BindInstance(config);
            Container.BindLarjeAdsProvider<AdsgramAdsService>();
        }
    }
}
