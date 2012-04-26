/* 
 * PROJECT: NyARToolkitCS
 * --------------------------------------------------------------------------------
 * This work is based on the original ARToolKit developed by
 *   Hirokazu Kato
 *   Mark Billinghurst
 *   HITLab, University of Washington, Seattle
 * http://www.hitl.washington.edu/artoolkit/
 *
 * The NyARToolkitCS is C# edition ARToolKit class library.
 * Copyright (C)2008-2009 Ryo Iizuka
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 * 
 * For further information please contact.
 *	http://nyatla.jp/nyatoolkit/
 *	<airmail(at)ebony.plala.or.jp> or <nyatla(at)nyatla.jp>
 * 
 */

using jp.nyatla.nyartoolkit.cs;
using jp.nyatla.nyartoolkit.cs.core;

namespace GoblinXNA.Device.Vision.Marker
{
    /// <summary>
    /// A full copy of NyARRasterFilter_ARToolkitThreshold from NyARToolkitCS library, but with modification
    /// for optimizing binarization of RGBA color routine.
    /// </summary>
    internal class Threshold : INyARRasterFilter_Rgb2Bin
    {
        protected int _threshold;
        private IdoThFilterImpl _do_threshold_impl;

        public Threshold(int i_threshold, int i_in_raster_type)
        {
            if (!initInstance(i_threshold, i_in_raster_type, NyARBufferType.INT1D_BIN_8))
            {
                throw new NyARException();
            }
        }
        public Threshold(int i_threshold, int i_in_raster_type, int i_out_raster_type)
        {
            if (!initInstance(i_threshold, i_in_raster_type, i_out_raster_type))
            {
                throw new NyARException();
            }
        }
        protected bool initInstance(int i_threshold, int i_in_raster_type, int i_out_raster_type)
        {
            switch (i_out_raster_type)
            {
                case NyARBufferType.INT1D_BIN_8:
                    switch (i_in_raster_type)
                    {
                        case NyARBufferType.BYTE1D_B8G8R8X8_32:
                            this._do_threshold_impl = new doThFilterImpl_BUFFERFORMAT_BYTE1D_B8G8R8X8_32();
                            break;
                        default:
                            return false; // Not supported
                    }
                    break;
                default:
                    return false; // Not supported
            }
            this._threshold = i_threshold;
            return true;
        }

        /**
         * 画像を２値化するための閾値。暗点<=th<明点となります。
         * @param i_threshold
         */
        public void setThreshold(int i_threshold)
        {
            this._threshold = i_threshold;
        }

        public void doFilter(INyARRgbRaster i_input, NyARBinRaster i_output)
        {
            NyARIntSize s = i_input.getSize();
            this._do_threshold_impl.doThFilter(i_input, 0, 0, s.w, s.h, this._threshold, i_output);
            return;
        }

        public void doFilter(INyARRgbRaster i_input, NyARIntRect i_area, NyARBinRaster i_output)
        {
            this._do_threshold_impl.doThFilter(i_input, i_area.x, i_area.y, i_area.w, i_area.h, this._threshold, i_output);
            return;
        }

        protected interface IdoThFilterImpl
        {
            void doThFilter(INyARRaster i_raster, int i_l, int i_t, int i_w, int i_h, int i_th, INyARRaster o_raster);
        }

        class doThFilterImpl_BUFFERFORMAT_BYTE1D_B8G8R8X8_32 : IdoThFilterImpl
        {
            public void doThFilter(INyARRaster i_raster, int i_l, int i_t, int i_w, int i_h, int i_th, INyARRaster o_raster)
            {
                byte[] input = (byte[])i_raster.getBuffer();
                int[] output = (int[])o_raster.getBuffer();
                NyARIntSize s = i_raster.getSize();
                int th = i_th;
                int skip_dst = (s.w - i_w);
                int skip_src = skip_dst * 4;
                int pix_count = i_w;
                int pix_mod_part = pix_count - (pix_count % 8);
                //左上から1行づつ走査していく
                int pt_dst = (i_t * s.w + i_l);
                int pt_src = pt_dst * 4;
                for (int y = i_h - 1; y >= 0; y -= 1)
                {
                    int x;
                    for (x = pix_count - 1; x >= pix_mod_part; x--)
                    {
                        output[pt_dst++] = input[pt_src + 0] <= th ? 0 : 1;
                        pt_src += 4;
                    }
                    for (; x >= 0; x -= 8)
                    {
                        output[pt_dst++] = input[pt_src + 0] <= th ? 0 : 1;
                        pt_src += 4;
                        output[pt_dst++] = input[pt_src + 0] <= th ? 0 : 1;
                        pt_src += 4;
                        output[pt_dst++] = input[pt_src + 0] <= th ? 0 : 1;
                        pt_src += 4;
                        output[pt_dst++] = input[pt_src + 0] <= th ? 0 : 1;
                        pt_src += 4;
                        output[pt_dst++] = input[pt_src + 0] <= th ? 0 : 1;
                        pt_src += 4;
                        output[pt_dst++] = input[pt_src + 0] <= th ? 0 : 1;
                        pt_src += 4;
                        output[pt_dst++] = input[pt_src + 0] <= th ? 0 : 1;
                        pt_src += 4;
                        output[pt_dst++] = input[pt_src + 0] <= th ? 0 : 1;
                        pt_src += 4;
                    }
                    //スキップ
                    pt_src += skip_src;
                    pt_dst += skip_dst;
                }
                return;
            }
        }
    }

}
