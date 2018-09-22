using System;
using System.Collections.Generic;

namespace Photon.Agent.Internal.Workers
{
    internal class PortPool<T>
    {
        private readonly Dictionary<int, T> map;
        private readonly object mapLock;

        private bool _isActive;
        private int _rangeMin, _rangeMax;

        public int RangeMin {
            get => _rangeMin;
            set {
                if (_isActive) throw new ApplicationException("Value cannot be modified when pool is active!");
                _rangeMin = value;
            }
        }

        public int RangeMax {
            get => _rangeMax;
            set {
                if (_isActive) throw new ApplicationException("Value cannot be modified when pool is active!");
                _rangeMax = value;
            }
        }


        public PortPool()
        {
            _rangeMin = 10930;
            _rangeMax = 10939;
            map = new Dictionary<int, T>();
            mapLock = new object();
        }

        public void Start()
        {
            lock (mapLock) {
                if (_isActive) throw new ApplicationException("Pool has already been started!");
                _isActive = true;

                map.Clear();
            }

            //...
        }

        public void Stop()
        {
            lock (mapLock) {
                if (!_isActive) return;
                _isActive = false;

                map.Clear();
            }

            //...
        }

        public bool Register(T item, out int port)
        {
            lock (mapLock) {
                if (!_isActive) throw new ApplicationException("Pool has not been started!");

                for (var i = _rangeMin; i <= _rangeMax; i++) {
                    if (map.ContainsKey(i)) continue;

                    port = i;
                    map[i] = item;
                    return true;
                }
            }

            port = 0;
            return false;
        }

        public void Release(int port)
        {
            lock (mapLock) {
                map.Remove(port);
            }
        }
    }
}
