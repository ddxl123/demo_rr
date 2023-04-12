
using System.ComponentModel;

public enum PositionBlockType
{
    [Description("截图左侧靠在屏幕最左边的")]
    left,
    [Description("截图顶部靠在屏幕最顶端的")]
    top,
    [Description("截图右侧靠在屏幕最右边的")]
    right,
    [Description("截图底部靠在屏幕最底端的")]
    down,
    [Description("截图左上角靠在屏幕最左上方的")]
    left_top,
    [Description("截图左下角靠在屏幕最左下方的")]
    left_down,
    [Description("截图右上角靠在屏幕最右上方的")]
    right_top,
    [Description("截图右下角靠在屏幕最右下方的")]
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


public enum EventKey
{
    [Description("单击鼠标左键")]
    mouseLeftClick,

    [Description("长按鼠标左键")]
    mouseLeftLongClick,

    [Description("双击鼠标左键")]
    mouseLeftDoubleClick,

    [Description("单击鼠标右键")]
    mouseRightClick,

}


public enum InOrderLoopStatus
{
    // 当前循环序号成功循环完成一次。
    complete,
    // 当前循环序号循环超时。
    timeout,
    // 当前循环序号跳转到其他地方了。
    jump,
    // 跳转的 Single 不存在。
    jump_fail,
}