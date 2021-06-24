using System.Collections.Generic;
using System.Timers;


/// <summary>
/// ��x���Ă��t���O���ݒ莞�Ԍ��(ms)������t���O�Ǘ��N���X
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


    //�Ǘ�����t���O
    private bool flag;
    //�^�C�}�[
    private Timer timer;
    //�C���^�[�o���̐ݒ�
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
    /// �t���O�𗧂Ăă^�C�}�[���X�^�[�g
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
