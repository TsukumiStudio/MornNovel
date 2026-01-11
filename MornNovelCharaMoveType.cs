using UnityEngine;

namespace MornLib
{
    public enum MornNovelCharaMoveType
    {
        [InspectorName("外側へ")] ToOuter,
        [InspectorName("内側へ")] ToInner,
        [InspectorName("左へ")] ToLeft,
        [InspectorName("右へ")] ToRight,
        [InspectorName("並行移動(既に表示しているとき)")] Slide,
    }
}