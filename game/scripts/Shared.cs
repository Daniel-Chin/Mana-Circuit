using System;

public class Shared
{
    public class ObjectStateIllegal : Exception { }
    public class ValueError : Exception { }
    public class PlayerCreatedEpsilonNaught : Exception { }
    public static Random Rand = new Random();
}
