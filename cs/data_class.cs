
using LiteDB;

public class Single
{
    public Single() { }

    // 需要默认值的成员
    public Single(
        string? imagePath,
        string? explain,
        double? similarityThreshold,
        PositionBlockType? positionBlockType,
        EventMode? eventMode,
        EventKey? eventKey,
        int? priority1,
        int? priority1LoopTimes,
        int? priority2,
        int? priority2CheckSecond,
        int? priority2TimeoutSecond,
        ObjectId? priority2toWhere,
        int? priority2ToNextAfterSecond
        )
    {
        ImagePath = imagePath;
        Explain = explain;
        SimilarityThreshold = similarityThreshold;
        PositionBlockType = positionBlockType;
        EventMode = eventMode;
        EventKey = eventKey;
        Priority1 = priority1;
        Priority1LoopTimes = priority1LoopTimes;
        Priority2 = priority2;
        Priority2CheckSecond = priority2CheckSecond;
        Priority2TimeoutSecond = priority2TimeoutSecond;
        Priority2ToWhere = priority2toWhere;
        Priority2ToNextAfterSecond = priority2ToNextAfterSecond;
    }
    public static string TABLE_NAME = "Single";

    [BsonId]
    public ObjectId? Id { get; set; }

    // 优先级，优先级越大，越优先。
    public int? Priority1 { get; set; }

    // 优先级，优先级越大，越优先。
    public int? Priority2 { get; set; }

    // 图片路径
    // 为空时，填充假图片
    public string? ImagePath { get; set; }

    // 解释
    public string? Explain { get; set; }

    // 相似阈值
    public double? SimilarityThreshold { get; set; }

    // 位置块类型
    public PositionBlockType? PositionBlockType { get; set; }

    // 属于哪个事件模式
    public EventMode? EventMode { get; set; }

    // Priority2 每多少秒检测1次
    public int? Priority2CheckSecond { get; set; }

    // Priority2 检测超过多少秒后，将跳转到其他地方检测
    // 负数表示无限。
    public int? Priority2TimeoutSecond { get; set; }

    // 跳转到哪里
    // 为 null 表示哪也不跳转
    public ObjectId? Priority2ToWhere { get; set; }

    public EventKey? EventKey { get; set; }

    // [Priority1] 重复循环多少次
    public int? Priority1LoopTimes { get; set; }

    // [Priority1] 循环完成后再等待多少秒后进入下一个循环序号
    // 若为0，则立即进入下一个循环序号
    public int? Priority2ToNextAfterSecond { get; set; }

    public override string ToString()
    {
        return $"[Single] Id: {Id}, Priority1: {Priority1}, Priority2: {Priority2}, " +
               $"ImagePath: {ImagePath ?? "null"}, Explain: {Explain ?? "null"}, " +
               $"SimilarityThreshold: {SimilarityThreshold}, PositionBlockType: {PositionBlockType}, " +
               $"EventMode: {EventMode}, Priority2CheckSecond: {Priority2CheckSecond}, " +
               $"Priority2Timeout: {Priority2TimeoutSecond}, ToWhere: {Priority2ToWhere}, " +
               $"EventKey: {EventKey}, Priority1LoopTimes: {Priority1LoopTimes}, " +
               $"Priority1ToNextAfterSecond: {Priority2ToNextAfterSecond}";
    }

}

