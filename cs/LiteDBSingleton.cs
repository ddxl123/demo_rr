using LiteDB;
using Tool;

public class KeyValue
{
    public KeyValue(string key, string value)
    {
        this.Key = key;
        this.Value = value;
    }

    public static string TABLE_NAME = "KeyValue";

    [BsonId]
    public ObjectId? Id { get; set; }

    public string Key { get; set; }
    public string Value { get; set; }

    public override string ToString()
    {
        return $"({Key}:{Value})";
    }
}
public sealed class Db
{
    private static readonly Db instance = new Db();
    private readonly LiteDatabase db;

    public static string LOOP_EVENT_TIME_CYCLE_SIMULTANEOUSLY = "loop_event_time_cycle_simultaneously";
    public static string LOOP_EVENT_TIME_CYCLE_IN_ORDER = "loop_event_time_cycle_in_order";

    private Db()
    {
        db = new LiteDatabase("db.db");
    }

    public static Db Instance
    {
        get
        {
            return instance;
        }
    }

    public LiteCollection<T> GetCollection<T>(string name)
    {
        return (LiteCollection<T>)db.GetCollection<T>(name, BsonAutoId.ObjectId);
    }

    public string? GetValueByKey(string key)
    {
        var c = Db.instance.GetCollection<KeyValue>(KeyValue.TABLE_NAME);
        var q = Query.EQ("Key", new BsonValue(key));
        return c.FindOne(q)?.Value;
    }

    public void SetKey(string key, string value)
    {
        var c = Db.instance.GetCollection<KeyValue>(KeyValue.TABLE_NAME);
        var q = Query.EQ("Key", new BsonValue(key));
        var result = c.FindOne(q);
        if (result == null)
        {
            c.Insert(new KeyValue(key, value));
        }
        else
        {
            result.Key = key;
            result.Value = value;
            c.Update(result);
        }
    }

    public void RemoveKey(string key)
    {
        var c = Db.instance.GetCollection<KeyValue>(KeyValue.TABLE_NAME);
        var q = Query.EQ("Key", new BsonValue(key));
        c.Delete(c.FindOne(q).Id);
    }


    // event_mode 值为对应的 [eventMode] 的 Query。
    public BsonExpression eventModeQuery(EventMode eventMode)
    {
        return Query.EQ("EventMode", new BsonValue(eventMode.ToString()));
    }

    public void Dispose()
    {
        db.Dispose();
    }

    // 设置多少秒循环一次
    //
    // 返回正确的时间s
    public double setLoopTime(string? second, EventMode eventMode)
    {
        int minSecond = 5;
        double findSecond;
        double.TryParse(second, out findSecond);
        if (findSecond < minSecond)
        {
            findSecond = minSecond;
        }

        if (eventMode == EventMode.Simultaneously)
        {
            SetKey(LOOP_EVENT_TIME_CYCLE_SIMULTANEOUSLY, findSecond.ToString());
        }
        else if (eventMode == EventMode.InOrder)
        {
            SetKey(LOOP_EVENT_TIME_CYCLE_IN_ORDER, findSecond.ToString());
        }
        else
        {
            throw new Exception($"未处理 {eventMode}");
        }
        Global.timer.Interval = findSecond * 1000;
        Global.timer.AutoReset = true;

        return findSecond;
    }


    // 对 [Single] 的序列化修改操作
    public void insertOrModifySingleEntity(Single item)
    {
        if (item.Id == null)
        {
            GetCollection<Single>(Single.TABLE_NAME).Insert(item);
        }
        else
        {
            Single? singleResult = GetCollection<Single>(Single.TABLE_NAME).FindById(item.Id);
            if (singleResult == null)
            {
                item.Id = null;
                GetCollection<Single>(Single.TABLE_NAME).Insert(item);
            }
            else
            {
                GetCollection<Single>(Single.TABLE_NAME).Update(item);
            }
        }
    }
}
