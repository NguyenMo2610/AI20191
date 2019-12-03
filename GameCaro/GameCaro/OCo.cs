using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameCaro
{
    class OCo
    {
        public const int _ChieuRong = 25;
        public const int _ChieuCao = 25;

        private int _Dong;// Easily locate chess piece

        public int Dong { get => _Dong; set => _Dong = value; }

        private int _Cot;// Easily locate chess piece

        public int Cot { get => _Cot; set => _Cot = value; }

        private Point _ViTri;

        public Point ViTri { get => _ViTri; set => _ViTri = value; }

        private int _SoHuu;// Who own this piece

        public int SoHuu { get => _SoHuu; set => _SoHuu = value; }

        public OCo()
        {

        }
        public OCo (int dong, int cot, Point vitri, int sohuu)
        {
            _Dong = dong;
            _Cot = cot;
            _ViTri = vitri;
            _SoHuu = sohuu;
        }
    }
}

