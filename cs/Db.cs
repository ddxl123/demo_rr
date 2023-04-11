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

    public static string SIMULTANEOUSLY_LOOP_EVENT_TIME_CYCLE = "simultaneously_loop_event_time_cycle";
    public static string IN_ORDER_LOOP_EVENT_WAIT_SECOND = "in_order_loop_event_wait_second";
    public static string IN_ORDER_LOOP_EVENT_TIMES = "in_order_loop_event_times";

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
    public BsonExpression eventModeQuery(EventMode eventModeValue)
    {
        return Query.EQ("EventMode", new BsonValue(eventModeValue));
    }
    public BsonExpression Priority1Query(int priority1Value)
    {
        return Query.EQ("Priority1", new BsonValue(priority1Value));
    }

    public BsonExpression Priority2Query(int priority2Value)
    {
        return Query.EQ("Priority2", new BsonValue(priority2Value));
    }

    public void Dispose()
    {
        db.Dispose();
    }

    // [T] - int 或 double
    public string SetSth<T>(string key, string? value, T min) where T : IComparable
    {
        if (typeof(T) == typeof(int))
        {
            int final;
            bool isSuccess = int.TryParse(value, out final);
            if (!isSuccess)
            {
                final = (int)(object)min;
            }
            if (final < (int)(object)min)
            {
                final = (int)(object)min;
            }

            SetKey(key, final.ToString());
            return final.ToString();
        }
        if (typeof(T) == typeof(double))
        {
            double final;
            bool isSuccess = double.TryParse(value, out final);
            if (!isSuccess)
            {
                final = (double)(object)min;
            }
            if (final < (double)(object)min)
            {
                final = (double)(object)min;
            }

            SetKey(key, final.ToString());
            return final.ToString();
        }
        throw new Exception($"未处理 {typeof(T)}");
    }



    public string GetSth<T>(string key, T min) where T : IComparable
    {
        // 若获取到的结果字符异常，则自动重新设置，例如为 null
        return SetSth<T>(key, GetValueByKey(key), min);
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

    public delegate T? OldValueRead<T>(Single oldSingle) where T : struct, IComparable?;

    // 把 [newValue] 值赋值给 [oldSingle]
    public delegate void NewValueToOldValue<T>(Single oldSingle, T newValue) where T : IComparable?;

    // [T] - int? 或 double?
    // min 和 max 都包含自身
    public string SetSingleSth<T>(
        Single single, OldValueRead<T> oldValueRead, string? newValue, NewValueToOldValue<T> newValueToOldValue, T min, T? max
        ) where T : struct, IComparable?
    {
        Single? result = GetCollection<Single>(Single.TABLE_NAME).FindById(new BsonValue(single.Id));
        if (result == null)
        {
            insertOrModifySingleEntity(single);
            result = single;
        }
        T? oldValue = oldValueRead(result!);

        if (typeof(T) == typeof(int))
        {
            int final;
            bool isSuccess = int.TryParse(newValue, out final);
            int intMin = (int)(object)min;
            int? intMax = (int?)(object?)max;
            int intOldValue = ((int?)(object?)oldValue) ?? intMin;
            if (!isSuccess)
            {
                final = intMin;
            }
            if (final < intMin)
            {
                final = intMin;
            }
            if (intMax.HasValue)
            {
                if (final > intMax)
                {
                    final = intMax.Value;
                }
            }

            if (final != intOldValue)
            {
                newValueToOldValue(result, (T)(object)final);
                insertOrModifySingleEntity(result);
            }
            return final.ToString();
        }

        if (typeof(T) == typeof(double))
        {
            double final;
            bool isSuccess = double.TryParse(newValue, out final);
            double intMin = (double)(object)min;
            double? intMax = (double?)(object?)max;
            double intOldValue = ((double?)(object?)oldValue) ?? intMin;
            if (!isSuccess)
            {
                final = intMin;
            }
            if (final < intMin)
            {
                final = intMin;
            }
            if (intMax.HasValue)
            {
                if (final > intMax)
                {
                    final = intMax.Value;
                }
            }


            if (final != intOldValue)
            {
                newValueToOldValue(result, (T)(object)final);
                insertOrModifySingleEntity(result);
            }
            return final.ToString();
        }


        throw new Exception($"未处理 {typeof(T)}");
    }


    public delegate T? NewValueRead<T>(Single? newSingle) where T : struct, IComparable?;

    // 获取 [Single] 类的某个属性值，若值为 null，则设置默认值为 [ming] 并保存。
    // [T] - int 或 double
    public string GetSingleSth<T>(
        Single single, OldValueRead<T> oldValueRead, NewValueRead<T> newValueRead, NewValueToOldValue<T> newValueToOldValue, T min, T? max
        ) where T : struct, IComparable?
    {
        Single? result = GetCollection<Single>(Single.TABLE_NAME).FindById(new BsonValue(single.Id));

        // 若获取到的结果字符异常，则自动重新设置，例如为 null
        return SetSingleSth<T>(single, oldValueRead, newValueRead(result)?.ToString(), newValueToOldValue, min, max);
    }

}
