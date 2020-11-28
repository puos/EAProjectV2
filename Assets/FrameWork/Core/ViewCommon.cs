using System;

public enum UiState
{
	None,
	Enable,
	Inited,
	WillDestroy,
}

public class Flags32
{
    public const int All = ~0;
}

public class ButtonId
{
	public const string Ok = @"Ok";
	public const string Cancel = @"Cancel";
	public const string Close = @"Close";
}

public class UiId
{
    int id = 0;
    string name;

    public UiId(string _name)
    {
        this.id = CRC32.GetHashForAnsi(_name);
        this.name = _name;
    }

    public void Set(string _name)
    {
        this.id = CRC32.GetHashForAnsi(_name);
        this.name = _name;
    }

    public void Set(UiId rhs)
    {
        Set(rhs.ToString());
    }

    public static bool operator ==(UiId lhs, UiId rhs)
    {
        return lhs.id == rhs.id;
    }

    public static bool operator !=(UiId lhs, UiId rhs)
    {
        return lhs.id != rhs.id;
    }

    public override bool Equals(object other)
    {
        var item = other as UiId;

        if (item == null)
        {
            return false;
        }

        return this.id == item.id;
    }

    public static UiId CreateInvalid()
    {
        return new UiId("Invalid");
    }

    public bool IsInvalid()
    {
        return name.Equals("Invalid");
    }

    public override int GetHashCode()
    {
        return id.GetHashCode();
    }

    public override string ToString()
    {
        return name;
    }
}



public class UiParam
{
	public Action<UiCtrlBase> onLoadCompleted;
    public bool isfade = true;

    public UiParam()
	{
	}
	
    public virtual void Clear()
    {       
    }

    public virtual void ResetParam()
    {
        isfade = true;
    }
}


public class UiResult
{
	public static int n = 10;
	public string btId;
	public UiCtrl uiCtrl = null;
	public UiResult()
	{
	}
	public UiResult(string btId)
	{
		this.btId = btId;
	}
    public virtual void Clear()
    {
        n = 10;
        btId = "";
        uiCtrl = null;
    }
}

public delegate void UiCallbackType<R>(R result);
