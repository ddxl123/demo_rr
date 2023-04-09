
using System.ComponentModel;

public enum PositionBlockType
{
    [Description("截选图左侧靠在屏幕最左边的")]
    left,
    [Description("截选图顶部靠在屏幕最顶端的")]
    top,
    [Description("截选图右侧靠在屏幕最右边的")]
    right,
    [Description("截选图底部靠在屏幕最底端的")]
    down,
    [Description("截选图左上角靠在屏幕最左上方的")]
    left_top,
    [Description("截选图左下角靠在屏幕最左下方的")]
    left_down,
    [Description("截选图右上角靠在屏幕最右上方的")]
    right_top,
    [Description("截选图右下角靠在屏幕最右下方的")]
    right_down,

}


public enum EventMode
{
    // 暂停状态
    None,
    // 同时检测
    Simultaneously,
    // 按顺序检测
    InOrder,
}

