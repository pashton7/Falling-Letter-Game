using Desktop_App;

namespace GameEngine
{
    public class CollisionInfo
    {
        public FallingLetter objA;
        public FallingLetter objB;
        public Vector2 Normal;
        public float Penetration; // how far into each other the objs are
        public Vector2 ContactPoint; // where the points touch

       
    }

}