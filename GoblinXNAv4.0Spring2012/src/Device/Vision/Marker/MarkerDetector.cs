﻿/* 
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

using jp.nyatla.nyartoolkit.cs.core;
using jp.nyatla.nyartoolkit.cs.utils;
using jp.nyatla.nyartoolkit.cs;

namespace GoblinXNA.Device.Vision.Marker
{
    /// <summary>
    /// A copy of NyARDetectMarker class from NyARToolkitCS library, but which changes for using optimized
    /// binary threshold class.
    /// </summary>
    /// <remarks>
    /// It's a full copy instead of extend since most of the members in NyARDetectMarker class is private.
    /// </remarks>
    internal class MarkerDetector
    {
	    private class RleDetector : NyARSquareContourDetector_Rle
	    {
		    //公開プロパティ
		    public NyARDetectMarkerResultStack result_stack=new NyARDetectMarkerResultStack(MarkerDetector.AR_SQUARE_MAX);
		    //参照インスタンス
		    public INyARRgbRaster _ref_raster;
		    //所有インスタンス
		    private INyARColorPatt _inst_patt;
		    private NyARMatchPattDeviationColorData _deviation_data;
		    private NyARMatchPatt_Color_WITHOUT_PCA[] _match_patt;
		    private NyARMatchPattResult __detectMarkerLite_mr=new NyARMatchPattResult();
		    private NyARCoord2Linear _coordline;

            public RleDetector(INyARColorPatt i_inst_patt, NyARCode[] i_ref_code, int i_num_of_code, NyARParam i_param)
                : base(i_param.getScreenSize())
		    {
			    int cw = i_ref_code[0].getWidth();
			    int ch = i_ref_code[0].getHeight();
			    //NyARMatchPatt_Color_WITHOUT_PCA[]の作成
			    this._match_patt=new NyARMatchPatt_Color_WITHOUT_PCA[i_num_of_code];
			    this._match_patt[0]=new NyARMatchPatt_Color_WITHOUT_PCA(i_ref_code[0]);
			    for (int i = 1; i < i_num_of_code; i++){
				    //解像度チェック
				    if (cw != i_ref_code[i].getWidth() || ch != i_ref_code[i].getHeight()) {
					    throw new NyARException();
				    }
				    this._match_patt[i]=new NyARMatchPatt_Color_WITHOUT_PCA(i_ref_code[i]);
			    }
			    this._inst_patt=i_inst_patt;
			    this._coordline=new NyARCoord2Linear(i_param.getScreenSize(),i_param.getDistortionFactor());
			    this._deviation_data=new NyARMatchPattDeviationColorData(cw,ch);
			    return;
		    }
		    private NyARIntPoint2d[] __ref_vertex=new NyARIntPoint2d[4];
		    /**
		     * 矩形が見付かるたびに呼び出されます。
		     * 発見した矩形のパターンを検査して、方位を考慮した頂点データを確保します。
		     */
		    protected override void onSquareDetect(NyARIntCoordinates i_coord,int[] i_vertex_index)
		    {
			    NyARMatchPattResult mr=this.__detectMarkerLite_mr;
			    //輪郭座標から頂点リストに変換
			    NyARIntPoint2d[] vertex=this.__ref_vertex;
			    vertex[0]=i_coord.items[i_vertex_index[0]];
			    vertex[1]=i_coord.items[i_vertex_index[1]];
			    vertex[2]=i_coord.items[i_vertex_index[2]];
			    vertex[3]=i_coord.items[i_vertex_index[3]];
    		
			    //画像を取得
			    if (!this._inst_patt.pickFromRaster(this._ref_raster,vertex)){
				    return;
			    }
			    //取得パターンをカラー差分データに変換して評価する。
			    this._deviation_data.setRaster(this._inst_patt);

			    //最も一致するパターンを割り当てる。
			    int square_index,direction;
			    double confidence;
			    this._match_patt[0].evaluate(this._deviation_data,mr);
			    square_index=0;
			    direction=mr.direction;
			    confidence=mr.confidence;
			    //2番目以降
			    for(int i=1;i<this._match_patt.Length;i++){
				    this._match_patt[i].evaluate(this._deviation_data,mr);
				    if (confidence > mr.confidence) {
					    continue;
				    }
				    // もっと一致するマーカーがあったぽい
				    square_index = i;
				    direction = mr.direction;
				    confidence = mr.confidence;
			    }
			    //最も一致したマーカ情報を、この矩形の情報として記録する。
			    NyARDetectMarkerResult result = this.result_stack.prePush();
			    result.arcode_id = square_index;
			    result.confidence = confidence;

			    NyARSquare sq=result.square;
			    //directionを考慮して、squareを更新する。
			    for(int i=0;i<4;i++){
				    int idx=(i+4 - direction) % 4;
				    this._coordline.coord2Line(i_vertex_index[idx],i_vertex_index[(idx+1)%4],i_coord,sq.line[i]);
			    }
			    for (int i = 0; i < 4; i++) {
				    //直線同士の交点計算
				    if(!sq.line[i].crossPos(sq.line[(i + 3) % 4],sq.sqvertex[i])){
					    throw new NyARException();//ここのエラー復帰するならダブルバッファにすればOK
				    }
			    }
		    }
		    public void init(INyARRgbRaster i_raster)
		    {
			    this._ref_raster=i_raster;
			    this.result_stack.clear();
    			
		    }
	    }	
	    private const int AR_SQUARE_MAX = 300;
	    private bool _is_continue = false;
	    private RleDetector _square_detect;
	    protected INyARTransMat _transmat;
	    private NyARRectOffset[] _offset;	


	    /**
	     * 複数のマーカーを検出し、最も一致するARCodeをi_codeから検索するオブジェクトを作ります。
	     * 
	     * @param i_param
	     * カメラパラメータを指定します。
	     * @param i_code
	     * 検出するマーカーのARCode配列を指定します。
	     * 配列要素のインデックス番号が、そのままgetARCodeIndex関数で得られるARCodeインデックスになります。 
	     * 例えば、要素[1]のARCodeに一致したマーカーである場合は、getARCodeIndexは1を返します。
	     * @param i_marker_width
	     * i_codeのマーカーサイズをミリメートルで指定した配列を指定します。 先頭からi_number_of_code個の要素には、有効な値を指定する必要があります。
	     * @param i_number_of_code
	     * i_codeに含まれる、ARCodeの数を指定します。
	     * @param i_input_raster_type
	     * 入力ラスタのピクセルタイプを指定します。この値は、INyARBufferReaderインタフェイスのgetBufferTypeの戻り値を指定します。
	     * @throws NyARException
	     */
        public MarkerDetector(NyARParam i_param, NyARCode[] i_code, double[] i_marker_width, int i_number_of_code, int i_input_raster_type)
	    {
		    initInstance(i_param,i_code,i_marker_width,i_number_of_code,i_input_raster_type);
		    return;
	    }
	    protected void initInstance(
		    NyARParam	i_ref_param,
		    NyARCode[]	i_ref_code,
		    double[]	i_marker_width,
		    int			i_number_of_code,
		    int i_input_raster_type)
	    {

		    NyARIntSize scr_size=i_ref_param.getScreenSize();
		    // 解析オブジェクトを作る
		    int cw = i_ref_code[0].getWidth();
		    int ch = i_ref_code[0].getHeight();

		    this._transmat = new NyARTransMat(i_ref_param);
		    //NyARToolkitプロファイル
		    this._square_detect =new RleDetector(new NyARColorPatt_Perspective_O2(cw, ch,4,25,i_input_raster_type),i_ref_code,i_number_of_code,i_ref_param);
		    this._tobin_filter=new Threshold(100,i_input_raster_type);

		    //実サイズ保存
		    this._offset = NyARRectOffset.createArray(i_number_of_code);
		    for(int i=0;i<i_number_of_code;i++){
			    this._offset[i].setSquare(i_marker_width[i]);
		    }
		    //２値画像バッファを作る
		    this._bin_raster=new NyARBinRaster(scr_size.w,scr_size.h);
		    return;		
	    }
    	
	    private NyARBinRaster _bin_raster;

	    private INyARRasterFilter_Rgb2Bin _tobin_filter;

	    /**
	     * i_imageにマーカー検出処理を実行し、結果を記録します。
	     * 
	     * @param i_raster
	     * マーカーを検出するイメージを指定します。
	     * @param i_thresh
	     * 検出閾値を指定します。0～255の範囲で指定してください。 通常は100～130くらいを指定します。
	     * @return 見つかったマーカーの数を返します。 マーカーが見つからない場合は0を返します。
	     * @throws NyARException
	     */
	    public int detectMarkerLite(INyARRgbRaster i_raster, int i_threshold)
	    {
		    // サイズチェック
		    if (!this._bin_raster.getSize().isEqualSize(i_raster.getSize())) {
			    throw new NyARException();
		    }

		    // ラスタを２値イメージに変換する.
		    ((Threshold)this._tobin_filter).setThreshold(i_threshold);
		    this._tobin_filter.doFilter(i_raster, this._bin_raster);

		    //detect
		    this._square_detect.init(i_raster);
		    this._square_detect.detectMarker(this._bin_raster);

		    //見付かった数を返す。
		    return this._square_detect.result_stack.getLength();
	    }

	    /**
	     * i_indexのマーカーに対する変換行列を計算し、結果値をo_resultへ格納します。 直前に実行したdetectMarkerLiteが成功していないと使えません。
	     * 
	     * @param i_index
	     * マーカーのインデックス番号を指定します。 直前に実行したdetectMarkerLiteの戻り値未満かつ0以上である必要があります。
	     * @param o_result
	     * 結果値を受け取るオブジェクトを指定してください。
	     * @throws NyARException
	     */
	    public void getTransmationMatrix(int i_index, NyARTransMatResult o_result)
	    {
		    NyARDetectMarkerResult result = this._square_detect.result_stack.getItem(i_index);
		    // 一番一致したマーカーの位置とかその辺を計算
		    if (_is_continue) {
			    _transmat.transMatContinue(result.square, this._offset[result.arcode_id], o_result,o_result);
		    } else {
			    _transmat.transMat(result.square, this._offset[result.arcode_id], o_result);
		    }
		    return;
	    }

	    /**
	     * i_indexのマーカーの一致度を返します。
	     * 
	     * @param i_index
	     * マーカーのインデックス番号を指定します。 直前に実行したdetectMarkerLiteの戻り値未満かつ0以上である必要があります。
	     * @return マーカーの一致度を返します。0～1までの値をとります。 一致度が低い場合には、誤認識の可能性が高くなります。
	     * @throws NyARException
	     */
	    public double getConfidence(int i_index)
	    {
		    return this._square_detect.result_stack.getItem(i_index).confidence;
	    }
	    /**
	     * i_indexのマーカーのARCodeインデックスを返します。
	     * 
	     * @param i_index
	     * マーカーのインデックス番号を指定します。 直前に実行したdetectMarkerLiteの戻り値未満かつ0以上である必要があります。
	     * @return
	     */
	    public int getARCodeIndex(int i_index)
	    {
		    return this._square_detect.result_stack.getItem(i_index).arcode_id;
	    }

	    /**
	     * getTransmationMatrixの計算モードを設定します。
	     * 
	     * @param i_is_continue
	     * TRUEなら、transMatContinueを使用します。 FALSEなら、transMatを使用します。
	     */
        public void setContinueMode(bool i_is_continue)
	    {
		    this._is_continue = i_is_continue;
	    }
    }

    class NyARDetectMarkerResult
    {
	    public int arcode_id;
	    public double confidence;

	    public NyARSquare square=new NyARSquare();
    }


    class NyARDetectMarkerResultStack : NyARObjectStack<NyARDetectMarkerResult>
    {
	    public NyARDetectMarkerResultStack(int i_length)
            :base()
	    {
		    this.initInstance(i_length);
		    return;
	    }
	    protected override NyARDetectMarkerResult createElement()
	    {
		    return new NyARDetectMarkerResult();
	    }	
    }
}
