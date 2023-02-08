using Godot;
using System;
using System.Collections.Generic;

public class EnumClass {
    public uint Id { get; private set; }
    private static HashSet<uint> AllId;
    static EnumClass() {
        AllId = new HashSet<uint>();
    }

    protected EnumClass(uint id) {
        if (AllId.Contains(id)) {
            throw new Exception("q85horeu");
        } else {
            AllId.Add(id);
            Id = id;
        }
    }
    
    public override bool Equals(object obj) {
        if (obj == null || GetType() != obj.GetType()) {
            return false;
        }
        return ((EnumClass) obj).Id == Id;
    }
    public override int GetHashCode()
    {
        return (int) Id;
    }
}
