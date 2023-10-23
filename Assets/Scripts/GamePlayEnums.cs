public enum SquareLocation
{
    square_one,
    square_two,
    square_three,
    square_four,
    out_of_square
}

public enum ShotType
{
    lob_shot,
    smash_shot
}

public enum BoostType
{
    speed,
    power,
    resetAll
}

public struct Stats
{
    public float speed { get; set; }
    public float power { get; set; }
    
    public float boostTime { get; set; }

    public Stats(float speed, float power, float boostTime)
    {
        this.speed = speed;
        this.power = power;
        this.boostTime = boostTime;
    }
    
}
}

public enum PrevBounce
{
    firstshot,
    square1,
    square2,
    square3,
    square4,
    other
}