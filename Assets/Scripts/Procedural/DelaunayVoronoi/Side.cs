﻿namespace Procedural {

public enum Side {
    LEFT = 0,
    RIGHT
}
public static class SideHelper 
{
    public static Side Other(Side leftRight) {
        return leftRight == Side.LEFT ? Side.RIGHT : Side.LEFT;
    }
}
}
