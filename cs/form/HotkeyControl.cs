using System.Text;

public class HotKeyControl : TextBox
{

    public string shortcut;
    protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
    {
        // 检测用户是否按下了任意组合键或单键
        if (keyData != Keys.None)
        {
            // 将快捷键转换为字符串
            shortcut = keyData.ToString();
            // 将字符串赋值给控件的Text属性
            this.Text = shortcut;
            // 返回true表示已经处理了该键盘事件
            return true;
        }
        // 否则调用基类的方法
        return base.ProcessCmdKey(ref msg, keyData);
    }

    public static void TriggerSendKeys(string shortcut)
    {
        try
        {
            var list = shortcut.Split(",").Reverse();
            var result = new StringBuilder();
            foreach (var item in list)
            {
                string i = item.Trim();
                if (i.Equals("Control"))
                {
                    result.Append("^");
                }
                else if (i.Equals("Alt"))
                {
                    result.Append("%");
                }
                else if (i.Equals("Shift"))
                {
                    result.Append("+");
                }
                else
                {
                    result.Append("{" + i + "}");
                }
            }
            SendKeys.Send(result.ToString().ToLower());
        }
        catch (System.Exception e)
        {
            MessageBox.Show($"无法识别快捷键：{e.Message}");
        }
    }
}