using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using DG.Tweening;

namespace MFarm.Light
{
    /// <summary>
    /// 单个灯光配置：一盏 Light2D 以及它在夜晚的目标亮度
    /// </summary>
    [Serializable]
    public class NightLightEntry
    {
        [Tooltip("需要控制的灯光子物体")]
        public Light2D light;

        [Tooltip("夜晚时该灯光渐亮到的目标强度")]
        public float targetIntensity = 1f;
    }

    /// <summary>
    /// 挂在需要夜晚开灯、白天关灯的物体上（例如路灯）。
    /// 夜晚把灯光子物体逐个激活，并在 3 秒内从 0 渐亮到各自配置的亮度；
    /// 粒子为可选配置，没有粒子时留空即可。
    /// </summary>
    public class NightLightGroup : MonoBehaviour
    {
        //灯光渐亮持续时间，需求固定为3秒
        private const float FadeDuration = 3f;

        [Header("夜晚灯光（数组顺序可自由调整）")]
        public NightLightEntry[] lights = new NightLightEntry[0];

        [Header("夜晚粒子（可选，没有就留空）")]
        public ParticleSystem nightParticle;

        //每盏灯对应的强度Tween，切换状态前需要先终止，避免多个Tween同时写入强度
        private Tween[] intensityTweens;

        //记录当前是否已经处于夜晚状态，避免每分钟事件重复触发渐亮
        private bool isNightActive;

        private void Awake()
        {
            //数组允许在Inspector里被清空，这里统一保证非空，后续可以直接按长度遍历
            if(lights == null)
            {
                lights = new NightLightEntry[0];
            }

            //Tween数组长度与灯光数组保持一致
            intensityTweens = new Tween[lights.Length];

            //场景初始默认按白天关闭，等场景加载完成后再按当前时间同步
            TurnOffLights();
            SetParticleActive(false);
        }

        private void OnEnable()
        {
            EventHandler.AfterSceneLoadedEvent += OnAfterSceneLoadedEvent;
            EventHandler.LightShiftChangeEvent += OnLightShiftChangeEvent;

            //组件重新启用时按当前时间同步一次
            RefreshState(IsNightTime());
        }

        private void OnDisable()
        {
            EventHandler.AfterSceneLoadedEvent -= OnAfterSceneLoadedEvent;
            EventHandler.LightShiftChangeEvent -= OnLightShiftChangeEvent;

            //组件停用时终止Tween并关灯，避免残留亮度和访问已销毁对象
            KillLightTweens();
            TurnOffLights();
            SetParticleActive(false);
            isNightActive = false;
        }

        private void OnDestroy()
        {
            KillLightTweens();
        }

        private void OnAfterSceneLoadedEvent()
        {
            //场景加载后按当前时间同步，避免读档或切场景后停在错误状态
            RefreshState(IsNightTime());
        }

        private void OnLightShiftChangeEvent(Season season, LightShift lightShift, float timeDifference)
        {
            //只在进入或离开夜晚时切换灯光
            RefreshState(lightShift == LightShift.Night);
        }

        /// <summary>
        /// 当前是否处于夜晚时段
        /// </summary>
        private bool IsNightTime()
        {
            if(TimeManager.Instance == null)
            {
                return false;
            }

            TimeSpan time = TimeManager.Instance.gameTime;
            return time >= Settings.nightTime || time < Settings.morningTime;
        }

        /// <summary>
        /// 按夜晚/白天状态切换灯光和粒子
        /// </summary>
        private void RefreshState(bool isNight)
        {
            if(isNightActive == isNight)
            {
                return;
            }

            isNightActive = isNight;
            KillLightTweens();

            if(isNight)
            {
                //夜晚：激活灯光并从0渐亮到各自目标亮度
                for(int i = 0; i < lights.Length; i++)
                {
                    NightLightEntry entry = lights[i];
                    if(entry == null || entry.light == null)
                    {
                        continue;
                    }

                    entry.light.gameObject.SetActive(true);
                    entry.light.intensity = 0f;

                    float targetIntensity = Mathf.Max(0f, entry.targetIntensity);
                    if(targetIntensity <= 0f)
                    {
                        continue;
                    }

                    intensityTweens[i] = DOTween.To(
                        () => entry.light.intensity,
                        value => entry.light.intensity = value,
                        targetIntensity,
                        FadeDuration);
                }
            }
            else
            {
                //白天：关闭所有灯光子物体
                TurnOffLights();
            }

            SetParticleActive(isNight);
        }

        /// <summary>
        /// 关闭所有灯光子物体并把强度归零
        /// </summary>
        private void TurnOffLights()
        {
            if(lights == null)
            {
                return;
            }

            for(int i = 0; i < lights.Length; i++)
            {
                NightLightEntry entry = lights[i];
                if(entry == null || entry.light == null)
                {
                    continue;
                }

                entry.light.intensity = 0f;
                entry.light.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// 开启或关闭夜晚粒子
        /// </summary>
        private void SetParticleActive(bool active)
        {
            if(nightParticle == null)
            {
                return;
            }

            if(active)
            {
                nightParticle.gameObject.SetActive(true);
                nightParticle.Clear(true);
                nightParticle.Play(true);
            }
            else
            {
                nightParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                //粒子如果和脚本在同一个物体上，不要禁用整个物体，否则脚本也会被停用
                if(nightParticle.gameObject != gameObject)
                {
                    nightParticle.gameObject.SetActive(false);
                }
            }
        }

        /// <summary>
        /// 终止所有灯光强度Tween
        /// </summary>
        private void KillLightTweens()
        {
            if(intensityTweens == null)
            {
                return;
            }

            for(int i = 0; i < intensityTweens.Length; i++)
            {
                intensityTweens[i]?.Kill();
                intensityTweens[i] = null;
            }
        }
    }
}
