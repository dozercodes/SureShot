/************************************************************************************ 
 * Copyright (c) 2008-2011, Columbia University
 * All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 *     * Redistributions of source code must retain the above copyright
 *       notice, this list of conditions and the following disclaimer.
 *     * Redistributions in binary form must reproduce the above copyright
 *       notice, this list of conditions and the following disclaimer in the
 *       documentation and/or other materials provided with the distribution.
 *     * Neither the name of the Columbia University nor the
 *       names of its contributors may be used to endorse or promote products
 *       derived from this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY COLUMBIA UNIVERSITY ''AS IS'' AND ANY
 * EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL <copyright holder> BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 * 
 * 
 * ===================================================================================
 * Authors: Ohan Oda (ohan@cs.columbia.edu) 
 * 
 *************************************************************************************/

#include <stdlib.h>
#include <vector>
#include <map>
#include "MarkerDetector.h"
#include "MultiMarker.h"
#include "MultiMarkerEx.h"

using namespace std;
using namespace alvar;

struct ALVARCamera
{
	Camera* cam;
	int width;
	int height;
};

vector<ALVARCamera> cams;
vector<MarkerDetector<MarkerData> *> markerDetectors;
vector<MultiMarker> multiMarkers;
IplImage image;
IplImage *hide_texture;
unsigned int hide_texture_size;
unsigned int channels;
double margin;
map<int, int> idTable;
map<int, int>::const_iterator foundPtr;
vector<int> foundMarkers;
double curMaxTrackError;

// Used for camera calibration
ProjPoints pp;
bool calibration_started;

extern "C"
{
	__declspec(dllexport) void alvar_init()
	{
		calibration_started = false;
	}

	// returns the ID of the added camera if succeeds, otherwise, returns -1
	__declspec(dllexport) int alvar_add_camera(char* calibFile, int width, int height)
	{
		int ret = -1;
		ALVARCamera camera;
		camera.cam = new Camera();
		if((calibFile != NULL) && camera.cam->SetCalib(calibFile, width, height))
			ret = cams.size();
		else
			camera.cam->SetRes(width, height);

		camera.width = width;
		camera.height = height;
		cams.push_back(camera);

		return ret;
	}

	__declspec(dllexport) void alvar_get_camera_projection(char* calibFile, int width, int height, 
		float farClip, float nearClip, double* projMat)
	{
		Camera cam;
		if(calibFile != NULL)
			cam.SetCalib(calibFile, width, height);
		
		cam.GetOpenglProjectionMatrix(projMat, width, height, farClip, nearClip);
	}

	__declspec(dllexport) int alvar_get_camera_params(int camID, double* projMat, double* fovX, double* fovY, float farClip, float nearClip)
	{
		if(camID >= cams.size())
			return -1;

		cams[camID].cam->GetOpenglProjectionMatrix(projMat, cams[camID].width, cams[camID].height, farClip, nearClip);

		*fovX = cams[camID].cam->GetFovX();
		*fovY = cams[camID].cam->GetFovY();
		return 0;
	}

	// returns the ID of the added marker detector
	__declspec(dllexport) int alvar_add_marker_detector(double markerSize, int markerRes = 5, double margin = 2)
	{
		MarkerDetector<MarkerData>* markerDetector = new MarkerDetector<MarkerData>();
		markerDetector->SetMarkerSize(markerSize, markerRes, margin);
		
		markerDetectors.push_back(markerDetector);
		return markerDetectors.size() - 1;
	}

	__declspec(dllexport) int alvar_set_marker_size(int detectorID, int markerID, double markerSize)
	{
		if(detectorID >= markerDetectors.size())
			return -1;

		markerDetectors[detectorID]->SetMarkerSizeForId(markerID, markerSize);
		return 0;
	}

	__declspec(dllexport) void alvar_add_multi_marker(char* filename)
	{
		MultiMarker marker;
		if(strstr(filename, ".xml") != NULL)
			marker.Load(filename, FILE_FORMAT_XML);
		else
			marker.Load(filename);
		multiMarkers.push_back(marker);
	}

	__declspec(dllexport) void alvar_detect_marker(int detectorID, int camID, int nChannels, 
		char* colorModel, char* channelSeq, char* imageData, int* interestedMarkerIDs, 
		int* numFoundMarkers, int* numInterestedMarkers, double maxMarkerError = 0.08, 
		double maxTrackError = 0.2)
	{
		if(detectorID >= markerDetectors.size() || camID >= cams.size())
			return;

		image.nSize = sizeof(IplImage);
		image.ID = 0;
		image.nChannels = nChannels;
		image.alphaChannel = 0;
		image.depth = IPL_DEPTH_8U;

		memcpy(&image.colorModel, colorModel, sizeof(char) * 4);
		memcpy(&image.channelSeq, channelSeq, sizeof(char) * 4);
		image.dataOrder = 0;

		image.origin = 0;
		image.align = 4;
		image.width = cams[camID].width;
		image.height = cams[camID].height;

		image.roi = NULL;
		image.maskROI = NULL;
		image.imageId = NULL;
		image.tileInfo = NULL;
		image.widthStep = cams[camID].width * nChannels;
		image.imageSize = cams[camID].height * image.widthStep;

		image.imageData = imageData;
		image.imageDataOrigin = NULL;

		markerDetectors[detectorID]->Detect(&image, cams[camID].cam, true, false, maxMarkerError, maxTrackError);
		curMaxTrackError = maxTrackError;
		*numFoundMarkers = markerDetectors[detectorID]->markers->size();

		int interestedMarkerNum = *numInterestedMarkers;
		int markerCount = 0;
		int tmpID = 0;
		foundMarkers.clear();
		int size = markerDetectors[detectorID]->markers->size();
		if(size > 0 && interestedMarkerNum > 0)
		{
			idTable.clear();
			for(int i = 0; i < size; ++i)
			{
				tmpID = (*(markerDetectors[detectorID]->markers))[i].GetId();
				idTable[tmpID] = i;
			}

			for(int i = 0; i < interestedMarkerNum; ++i)
			{
				foundPtr = idTable.find(interestedMarkerIDs[i]);
				if(foundPtr != idTable.end())
				{
					foundMarkers.push_back(foundPtr->second);
					markerCount++;
				}
			}
		}

		*numInterestedMarkers = markerCount;
	}

	__declspec(dllexport) void alvar_get_poses(int detectorID, int* ids, double* poseMats)
	{
		if(detectorID >= markerDetectors.size())
			return;

		int size = foundMarkers.size();
		if(size == 0)
			return;

		double mat[16];
		int textureIndex = 0;
		for(size_t i = 0; i < foundMarkers.size(); ++i)
		{
			Pose p;
			ids[i] = (*(markerDetectors[detectorID]->markers))[foundMarkers[i]].GetId();
			p = (*(markerDetectors[detectorID]->markers))[foundMarkers[i]].pose;

			p.GetMatrixGL(mat);
			memcpy(poseMats + i * 16, &mat, sizeof(double) * 16);
		}
	}

	__declspec(dllexport) void alvar_get_multi_marker_poses(int detectorID, int camID, bool detectAdditional,
		int* ids, double* poseMats, double* errors)
	{
		if(detectorID >= markerDetectors.size() || camID >= cams.size())
			return;

		int size = markerDetectors[detectorID]->markers->size();
		if(size == 0)
			return;

		double mat[16];
		int textureIndex = 0;
		for(int i = 0; i < multiMarkers.size(); ++i)
		{
			ids[i] = i;
			Pose pose;

			if(detectAdditional)
			{
				errors[i] = multiMarkers.at(i).Update(markerDetectors[detectorID]->markers, 
					cams[camID].cam, pose);
				multiMarkers.at(i).SetTrackMarkers(*markerDetectors[detectorID], cams[camID].cam, pose);
				markerDetectors[detectorID]->DetectAdditional(&image, cams[camID].cam, false, curMaxTrackError);
			}

			errors[i] = multiMarkers.at(i).Update(markerDetectors[detectorID]->markers, cams[camID].cam, pose);
			pose.GetMatrixGL(mat);
			memcpy(poseMats + i * 16, &mat, sizeof(double) * 16);
		}
	}

	__declspec(dllexport) bool alvar_calibrate_camera(int camID, int nChannels, char* colorModel, char* channelSeq,
		char* imageData, double etalon_square_size, int etalon_rows, int etalon_columns)
	{
		if(camID >= cams.size())
			return false;

		image.nSize = sizeof(IplImage);
		image.ID = 0;
		image.nChannels = nChannels;
		image.alphaChannel = 0;
		image.depth = IPL_DEPTH_8U;

		memcpy(&image.colorModel, colorModel, sizeof(char) * 4);
		memcpy(&image.channelSeq, channelSeq, sizeof(char) * 4);
		image.dataOrder = 0;

		image.origin = 0;
		image.align = 4;
		image.width = cams[camID].width;
		image.height = cams[camID].height;

		image.roi = NULL;
		image.maskROI = NULL;
		image.imageId = NULL;
		image.tileInfo = NULL;
		image.widthStep = cams[camID].width * nChannels;
		image.imageSize = cams[camID].height * image.widthStep;

		image.imageData = imageData;
		image.imageDataOrigin = NULL;

		bool ret = pp.AddPointsUsingChessboard(&image, etalon_square_size, etalon_rows, etalon_columns, false);
		if(ret)
			calibration_started = true;
		return ret;
	}

	__declspec(dllexport) bool alvar_finalize_calibration(int camID, char* calibrationFilename)
	{
		if(!calibration_started || (camID >= cams.size()))
			return false;

		cams[camID].cam->Calibrate(pp);
		pp.Reset();
	
		bool ret = cams[camID].cam->SaveCalib(calibrationFilename);
		if(ret)
			calibration_started = false;
		return ret;
	}
}