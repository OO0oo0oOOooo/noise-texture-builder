#ifndef MAFFS
#define MAFFS

// 2+2=5

float InverseLerp(float a, float b, float v)
{
    return (v - a) / (b - a);
}

float remap(float origFrom, float origTo, float targetFrom, float targetTo, float value)
{
    float rel = InverseLerp(origFrom, origTo, value);
    return lerp(targetFrom, targetTo, rel);
}

float map(float origFrom, float origTo, float targetFrom, float targetTo, float value)
{
    float rel = InverseLerp(origFrom, origTo, value);
    return rel * (targetTo - targetFrom) + targetFrom;
}

#endif