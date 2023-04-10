using System.Numerics;
using System.Timers;
using Tool;

public class TimerElapser
{

    public static void TimerElapsed(object? sender, ElapsedEventArgs e)
    {
        var singles = Db.Instance.GetCollection<Single>(Single.TABLE_NAME).FindAll().OrderByDescending((e) => e.Priority1);

        Rectangle? r = GetRectangle(singles);
        if (r.HasValue)
        {
            // await Task.Delay(1000);
            Tool.Clicker.Click(r.Value);
        }
    }


    public static Rectangle? GetRectangle(IOrderedEnumerable<Single> singles)
    {
        foreach (var single in singles)
        {
            string fullScreanImagePath = K.ASSETS_SINGLE + "/full_screan.png";
            Tool.Capturer.CaptureFullScreen(fullScreanImagePath, null);
            Rectangle fullScreanSize = Screen.GetBounds(Point.Empty);
            if (single.ImagePath != null)
            {
                List<Rectangle> rList = Tool.ImageMatcher.MatchImage(single.ImagePath, fullScreanImagePath, single.SimilarityThreshold);
                if (rList.Count != 0)
                {
                    switch (single.PositionBlockType)
                    {
                        case PositionBlockType.left:
                            return rList.OrderBy((e) => e.Left).First();
                        case PositionBlockType.right:
                            return rList.OrderByDescending((e) => e.Right).First();
                        case PositionBlockType.top:
                            return rList.OrderBy((e) => e.Top).First();
                        case PositionBlockType.down:
                            return rList.OrderByDescending((e) => e.Bottom).First();
                        case PositionBlockType.left_top:
                            return rList.OrderBy((e) => new Vector2(e.Left, e.Top).Length()).First();
                        case PositionBlockType.left_down:
                            return rList.OrderBy((e) => new Vector2(e.Left, fullScreanSize.Height - e.Bottom).Length()).First();
                        case PositionBlockType.right_top:
                            return rList.OrderBy((e) => new Vector2(fullScreanSize.Width - e.Right, e.Top).Length()).First();
                        case PositionBlockType.right_down:
                            return rList.OrderBy((e) => new Vector2(fullScreanSize.Width - e.Right, fullScreanSize.Height - e.Bottom).Length()).First();
                        default:
                            throw new Exception($"未知：{single.PositionBlockType}");
                    }
                }
            }
        }
        return null;
    }
}