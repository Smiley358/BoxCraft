using System.Collections.Generic;
using System.Timers;


/// <summary>
/// 一度立てたフラグが設定時間後に(ms)下がるフラグ管理クラス
/// </summary>
public class TimeSpanFlag
{
    public static implicit operator bool (TimeSpanFlag timeSpanFlag)
    {
        return timeSpanFlag.flag;
    }
    public static bool operator ==(TimeSpanFlag timeSpanFlag, bool flag)
    {
        return timeSpanFlag.flag == flag;
    }
    public static bool operator !=(TimeSpanFlag timeSpanFlag, bool flag)
    {
        return timeSpanFlag.flag != flag;
    }
    public override bool Equals(object obj)
    {
        return obj is TimeSpanFlag flag &&
               this.flag == flag.flag;
    }
    public override int GetHashCode()
    {
        int hashCode = 1399872145;
        hashCode = hashCode * -1521134295 + flag.GetHashCode();
        hashCode = hashCode * -1521134295 + EqualityComparer<Timer>.Default.GetHashCode(timer);
        return hashCode;
    }
    public override string ToString()
    {
        return timer.Interval.ToString() + ":" + flag.ToString();
    }


    //管理するフラグ
    private bool flag;
    //タイマー
    private Timer timer;
    //インターバルの設定
    public double Interval { 
        get
        {
            return timer.Interval;
        }
        set
        {
            timer.Interval = value;
        }
    } 

    public TimeSpanFlag()
    {
        timer = new Timer(0);
    }

    public TimeSpanFlag(long timeSpan)
    {
        timer = new Timer(timeSpan);
    }

    /// <summary>
    /// フラグを立ててタイマーをスタート
    /// </summary>
    public void Begin()
    {
        if (!flag)
        {
            flag = true;
            timer.Elapsed += (sender, e) =>
            {
                flag = false;
                timer.Stop();
            };
            timer.Start();
        }
    }
}
