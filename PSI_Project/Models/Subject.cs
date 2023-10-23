﻿namespace PSI_Project.Models;

public class Subject : BaseEntity, IComparable<Subject>, IEquatable<Subject>
{
    public string Name { get; init; }
    
    public Subject(string name)
    {
        Name = name;
    }

    public int CompareTo(Subject other)
    {
        return Name.CompareTo(other.Name);
    }

    public bool Equals(Subject? other)
    {
        if (other == null)
            return false;

        return Name == other.Name;
    }

    public override bool Equals(object? obj)
    {
        if (obj is Subject)
            return Equals((Subject)obj);

        return false;
    }

    public override int GetHashCode()
    {
        return Name.GetHashCode();
    }
}