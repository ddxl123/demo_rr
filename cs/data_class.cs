
using LiteDB;

public class Single
{
    public Single() { }
    public Single(int priority1, int? priority2, string? imagePath, string? explain, EventMode eventMode)
    {
        this.Priority1 = priority1;
        this.Priority2 = priority2;
        this.ImagePath = imagePath;
        this.Explain = explain;
        this.EventMode = eventMode;
    }

    public static string TABLE_NAME = "Single";

    [BsonId]
    public ObjectId? Id { get; set; }

    // 优先级，优先级越大，越优先。
    public int Priority1 { get; set; }

    // 优先级，优先级越大，越优先。
    public int? Priority2 { get; set; }

    // 图片路径
    // 为空时，填充假图片
    public string? ImagePath { get; set; }

    // 解释
    public string? Explain { get; set; }

    // 相似阈值
    public double SimilarityThreshold { get; set; } = 0.7;

    // 位置块类型
    public PositionBlockType PositionBlockType { get; set; } = PositionBlockType.left_top;

    // 属于哪个事件模式
    public EventMode EventMode { get; set; }

    // 循环次数
    public int LoopTimes { get; set; } = 1;

    // 每多少秒检测1次
    public int CheckSecond { get; set; } = 1;

    // 检测超过多少秒后，将跳转到其他地方检测
    // 负数表示无限。
    public int Timeout { get; set; } = -1;

    // 跳转到哪里
    // 为 null 表示哪也不跳转
    public ObjectId? toWhere { get; set; }

    public EventKey? eventKey { get; set; }

    // [Priority1] 重复循环多少次
    public int Priority1LoopTimes { get; set; } = 1;

    // [Priority1] 循环完成后再等待多少秒后进入下一个循环序号
    // 若为-1，则立即进入下一个循环序号
    public int Priority1ToNextAfterSecond { get; set; } = -1;

    public override string ToString()
    {
        return $"[Single] Id: {Id}, Priority1: {Priority1}, Priority2: {Priority2}, ImagePath: {ImagePath ?? "null"}, Explain: {Explain ?? "null"}, SimilarityThreshold: {SimilarityThreshold}, PositionBlockType: {PositionBlockType}, EventMode: {EventMode}";
    }
}


