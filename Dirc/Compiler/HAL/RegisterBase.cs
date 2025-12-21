namespace Dirc.HAL;

public class RegisterBase(int id, string name, bool alwaysAvailable)
{
    public int ID = id;
    public string Name = name;
    public bool AlwaysAvailable = alwaysAvailable;

    public static bool operator ==(RegisterBase ob1, RegisterBase ob2)
    {
        return ob1.ID == ob2.ID;
    }

    public static bool operator !=(RegisterBase ob1, RegisterBase ob2)
    {
        return ob1.ID != ob2.ID;
    }

    public override bool Equals(object? obj)
    {
        if (obj is not RegisterBase reg) return false;
        return ID == reg.ID;
    }

    public override int GetHashCode()
    {
        return ID;
    }
}
