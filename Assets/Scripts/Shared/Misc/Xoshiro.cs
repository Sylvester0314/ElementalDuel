using System;

namespace Shared.Misc
{
    public class Xoshiro
    {
        private ulong _s0;
        private ulong _s1;
        private ulong _s2;
        private ulong _s3;

        public static Xoshiro Create()
        {
            var guid = Guid.NewGuid().ToByteArray();
            var seed = BitConverter.ToUInt64(guid, 0); 
            return new Xoshiro(seed);
        }
        
        public Xoshiro(ulong seed)
        {
            _s0 = seed ^ 0x9E3779B97F4A7C15;
            _s1 = seed >> 1;
            _s2 = seed << 1;
            _s3 = seed >> 3;
        }

        public ulong Next()
        {
            var result = _s0 + _s3;
            var t = _s1 << 17;

            _s2 ^= _s0;
            _s3 ^= _s1;
            _s1 ^= _s2;
            _s0 ^= _s3;

            _s2 ^= t;
            _s3 = (_s3 << 45) | (_s3 >> (64 - 45));
            return result;
        }

        public float NextFloat()
        {
            return (Next() >> 11) * (1.0f / (1UL << 53));
        }

        public int NextInt(int min, int max)
        {
            return (int)((ulong)min + Next() % (ulong)(max - min));
        }
        
        public int NextInt(int max)
        {
            return NextInt(0, max);
        }
    }
}