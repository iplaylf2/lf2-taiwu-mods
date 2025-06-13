using GameData.Utilities;
using HarmonyLib;
using Redzen.Numerics.Distributions.Float;
using Redzen.Random;

namespace TiredSL.Backend.Random;

public static class RandomImprove
{
    [HarmonyPatch(typeof(RedzenHelper), nameof(RedzenHelper.SkewDistribute))]
    [HarmonyPrefix]
    public static bool SkewDistribute(ref int __result, IRandomSource randomSource,
        float mean, float stdDev, float skewness, int min, int max)
    {
        __result = SkewDistribute(randomSource, mean, stdDev, skewness, min, max);

        return false;
    }

    public static int SkewDistribute(IRandomSource randomSource, float mean, float stdDev, float skewParam,
        int min = int.MinValue, int max = int.MaxValue)
    {
        // skewParam 与偏度正相关，但不等同于偏度。
        Tester.Assert(Math.Abs(skewParam) > 1.0f);

        // 标准正态采样
        float x = ZigguratGaussian.Sample(randomSource);
        float k = Math.Abs(skewParam);
        bool isPositiveSkew = skewParam > 0;

        // ========== 核心数学推导部分 ==========
        // 过渡函数：Sigmoid导数特性（避免经验参数）
        float transition = 1.0f / (1.0f + MathF.Exp(-MathF.Tau * x)); // Tau=2π≈6.283，数学常数

        // 缩放因子期望（解析积分结果）
        float E_scale = 0.5f * (1 + k);

        // 缩放因子方差（基于Sigmoid方差特性）
        float Var_scale = (k - 1.0f) * (k - 1.0f) * 0.25f; // 严格来自∫(σ(x)-0.5)^2 φ(x)dx

        // 调整后变量期望
        float sqrt2OverPi = MathF.Sqrt(2.0f / MathF.PI);
        float mu_adj = MathF.Sign(skewParam) * sqrt2OverPi * (E_scale - 1.0f);

        // 调整后变量方差（二阶矩展开）
        float E_x2s2 = E_scale * E_scale + Var_scale;
        float sigma_adj = MathF.Sqrt(E_x2s2 - mu_adj * mu_adj);

        // ========== 实施变换 ==========
        float scale = isPositiveSkew ?
            1.0f + (k - 1.0f) * transition :
            1.0f + (k - 1.0f) * (1.0f - transition);

        float scaled = x * scale;
        float normalized = (scaled - mu_adj) / sigma_adj;

        // 最终输出
        int result = (int)MathF.Round(mean + normalized * stdDev);

        return Math.Clamp(result, min, max);
    }
}
