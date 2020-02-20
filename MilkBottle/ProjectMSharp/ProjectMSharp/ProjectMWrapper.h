#pragma once

#include "..\libprojectM\projectM.hpp"
#include "Renderer.hpp"
#include <string.h>
#include <msclr\marshal_cppstd.h>
using namespace System;
using namespace System::Runtime::InteropServices;
#include "SDL.h"

#include <glew.h>
#include <gl\GL.h>
#include <iostream>

#pragma unmanaged
typedef int(__stdcall *PresetSwitchedCallback)(bool, size_t);

class projectMEvents : public projectM {
public:
	PresetSwitchedCallback presetChangedCallback;

	projectMEvents(std::string config_file, int flags = FLAG_NONE) : projectM(config_file, flags) {
		presetChangedCallback = NULL;
	}

	projectMEvents(Settings settings, int flags = FLAG_NONE) : projectM(settings, flags) {
		presetChangedCallback = NULL;
	}

	void presetSwitchedEvent(bool isHardCut, size_t index) const {
		if (presetChangedCallback != NULL) {
			presetChangedCallback(isHardCut, index);
		}
	};
};

#pragma managed
public
ref class ProjectMSettings {
public:
	ProjectMSettings()
			: MeshWidth(32),
				MeshHeight(24),
				FrameRate(35),
				TextureSize(512),
				WindowWidth(512),
				WindowHeight(512),
				PresetFolder(""),
				TitleFontURL(""),
				MenuFontURL(""),
				DataFolder(""),
				SmoothPresetDuration(10),
				PresetDuration(15),
				BeatSensitivity(4.0),
				AspectCorrection(true),
				ShuffleEnabled(false),
				SoftCutRatingsEnabled(false)
	{
	}

	int MeshWidth;
	int MeshHeight;
	int FrameRate;
	int TextureSize;
	int WindowWidth;
	int WindowHeight;
	System::String ^ PresetFolder;
	System::String ^ TitleFontURL;
	System::String ^ MenuFontURL;
	System::String ^ DataFolder;
	int SmoothPresetDuration;
	int PresetDuration;
	float BeatSensitivity;
	bool AspectCorrection;
	bool ShuffleEnabled;
	bool SoftCutRatingsEnabled;
};

public
delegate void PresetSwitchedDelegate(bool isHardCut, size_t toIndex);

public
ref class ProjectMWrapper {
private:
	SDL_Window		*mWindow;
	SDL_GLContext	mGlContext;
	projectMEvents *mProjectM;
	GCHandle		mPresetSwitchedHandle;

public:
	ProjectMWrapper() {
		mProjectM = NULL;
		mWindow = NULL;
	}

	~ProjectMWrapper() {
		if (mProjectM != NULL) {
			delete mProjectM;
		}

		if (mPresetSwitchedHandle.IsAllocated) {
			mPresetSwitchedHandle.Free();
		}
	}

	void setPresetCallback(PresetSwitchedDelegate ^ presetSwitchDelegate) {
		if (mPresetSwitchedHandle.IsAllocated) {
			mPresetSwitchedHandle.Free();
		}

		mPresetSwitchedHandle = GCHandle::Alloc(presetSwitchDelegate);
		IntPtr ip = Marshal::GetFunctionPointerForDelegate(presetSwitchDelegate);

		mProjectM->presetChangedCallback = static_cast<PresetSwitchedCallback>(ip.ToPointer());
	}

	bool isInitialized() { return mProjectM != NULL; }
	/*
		void initialize(System::String ^ config_file, int flags, System::IntPtr hostWindow)	{
			msclr::interop::marshal_context context;
			std::string configString = context.marshal_as<std::string>(config_file);

			SDL_Init(SDL_INIT_VIDEO);

			SDL_Rect initialWindowBounds;
			SDL_GetDisplayUsableBounds(0, &initialWindowBounds);

			// Disabling compatibility profile
			SDL_GL_SetAttribute(SDL_GL_CONTEXT_MAJOR_VERSION, 3);
			SDL_GL_SetAttribute(SDL_GL_CONTEXT_MINOR_VERSION, 2);
			SDL_GL_SetAttribute(SDL_GL_CONTEXT_PROFILE_MASK, SDL_GL_CONTEXT_PROFILE_CORE);

			// from:
			//
		https://gamedev.stackexchange.com/questions/110205/context-is-null-with-sdl-createwindowfrom-win32
			SDL_Window *pSampleWin = SDL_CreateWindow("", 0, 0, 1, 1, SDL_WINDOW_OPENGL |
		SDL_WINDOW_HIDDEN);

			char sBuf[32];
			sprintf_s<32>(sBuf, "%p", pSampleWin);

			SDL_SetHint(SDL_HINT_VIDEO_WINDOW_SHARE_PIXEL_FORMAT, sBuf);
			mWindow = SDL_CreateWindowFrom(hostWindow.ToPointer());
			SDL_SetHint(SDL_HINT_VIDEO_WINDOW_SHARE_PIXEL_FORMAT, nullptr);
			//		SDL_DestroyWindow(pSampleWin);

			const char *sdl_error = SDL_GetError();

			// Create an OpenGL context associated with the window.
			mGlContext = SDL_GL_CreateContext(mWindow);
			SDL_GL_MakeCurrent(mWindow, mGlContext); // associate GL context with main window

			int avsync = SDL_GL_SetSwapInterval(-1); // try to enable adaptive vsync
			if (avsync == -1) {
				SDL_GL_SetSwapInterval(1); // enable updates synchronized with vertical retrace
			}

			SDL_Log("GL_VERSION: %s", glGetString(GL_VERSION));
			SDL_Log("GL_SHADING_LANGUAGE_VERSION: %s", glGetString(GL_SHADING_LANGUAGE_VERSION));
			SDL_Log("GL_VENDOR: %s", glGetString(GL_VENDOR));

			mProjectM = new projectMEvents(configString, flags);
		}
	*/
	/*
		void initialize(int width, int height, int flags) {
			std::string base_path = SDL_GetBasePath();

			float heightWidthRatio = (float)height / (float)width;
			projectM::Settings settings;
			settings.windowWidth = width;
			settings.windowHeight = height;
			settings.meshX = 128;
			settings.meshY = settings.meshX * heightWidthRatio;
			settings.fps = 60;
			settings.smoothPresetDuration = 3; // seconds
			settings.presetDuration = 22;			 // seconds
			settings.beatSensitivity = 0.8;
			settings.aspectCorrection = 1;
			settings.shuffleEnabled = 1;
			settings.softCutRatingsEnabled = 1; // ???
			// get path to our app, use CWD or resource dir for presets/fonts/etc
			settings.presetURL = base_path + "presets";
			settings.menuFontURL = base_path + "fonts/Vera.ttf";
			settings.titleFontURL = base_path + "fonts/Vera.ttf";
			// init with settings
			mProjectM = new projectMEvents(settings, flags);
		}
	*/
	void initialize(System::String ^ config_file) {
		msclr::interop::marshal_context context;
		std::string configString = context.marshal_as<std::string>(config_file);

		if (mProjectM != NULL) {
			delete mProjectM;
		}

		mProjectM = new projectMEvents(configString, 0);
	}

	void initialize(ProjectMSettings ^ settings) {
		projectM::Settings nativeSettings;

		nativeSettings.meshX = settings->MeshWidth;
		nativeSettings.meshY = settings->MeshHeight;
		nativeSettings.fps = settings->FrameRate;
		nativeSettings.textureSize = settings->TextureSize;
		nativeSettings.windowHeight = settings->WindowHeight;
		nativeSettings.windowWidth = settings->WindowWidth;
		nativeSettings.smoothPresetDuration = settings->SmoothPresetDuration;
		nativeSettings.presetDuration = settings->PresetDuration;
		nativeSettings.beatSensitivity = settings->BeatSensitivity;
		nativeSettings.aspectCorrection = settings->AspectCorrection;
		nativeSettings.shuffleEnabled = settings->ShuffleEnabled;
		nativeSettings.softCutRatingsEnabled = settings->SoftCutRatingsEnabled;

		msclr::interop::marshal_context context;
		nativeSettings.presetURL = context.marshal_as<std::string>(settings->PresetFolder);
		nativeSettings.titleFontURL = context.marshal_as<std::string>(settings->TitleFontURL);
		nativeSettings.menuFontURL = context.marshal_as<std::string>(settings->MenuFontURL);
		nativeSettings.datadir = context.marshal_as<std::string>(settings->DataFolder);

		if (mProjectM != NULL) {
			delete mProjectM;
		}
		
		mProjectM = new projectMEvents(nativeSettings, 0);
	}

	/*
		void initOpenGl() {
			SDL_Init(SDL_INIT_VIDEO);

			SDL_Rect initialWindowBounds;
			SDL_GetDisplayUsableBounds(0, &initialWindowBounds);

			// Disabling compatibility profile
			SDL_GL_SetAttribute(SDL_GL_CONTEXT_MAJOR_VERSION, 3);
			SDL_GL_SetAttribute(SDL_GL_CONTEXT_MINOR_VERSION, 2);
			SDL_GL_SetAttribute(SDL_GL_CONTEXT_PROFILE_MASK, SDL_GL_CONTEXT_PROFILE_CORE);

			int height = initialWindowBounds.h;
			int width = initialWindowBounds.w;
			mWindow = SDL_CreateWindow("projectM", width / 4, height / 4, width / 2, height / 2,
		SDL_WINDOW_OPENGL | SDL_WINDOW_RESIZABLE); SDL_SetWindowTitle(mWindow, "projectM Visualizer");

			// Create an OpenGL context associated with the window.
			mGlContext = SDL_GL_CreateContext(mWindow);
			SDL_GL_MakeCurrent(mWindow, mGlContext); // associate GL context with main window

			int avsync = SDL_GL_SetSwapInterval(-1); // try to enable adaptive vsync
			if (avsync == -1) {
				SDL_GL_SetSwapInterval(1); // enable updates synchronized with vertical retrace
			}

			SDL_Log("GL_VERSION: %s", glGetString(GL_VERSION));
			SDL_Log("GL_SHADING_LANGUAGE_VERSION: %s", glGetString(GL_SHADING_LANGUAGE_VERSION));
			SDL_Log("GL_VENDOR: %s", glGetString(GL_VENDOR));

			GLenum error = glGetError();
			if (error != GL_NO_ERROR) {
				_RPTN(_CRT_WARN, "OpenGL error after initialization: (%d) - %s\n", error,
		gluErrorString(error));
			}
		}
	*/
	void updateWindowSize(int width, int height) { mProjectM->projectM_resetGL(width, height); }

	void resetTextures() { mProjectM->projectM_resetTextures(); }
	/*
		void setWindowTitle(System::String ^ title) {
			msclr::interop::marshal_context context;
			std::string standardString = context.marshal_as<std::string>(title);

			mProjectM->projectM_setTitle(standardString);
		}
	*/
	void renderFrame() {
		if (isInitialized()) {
			glClearColor(0.0, 0.0, 0.0, 0.0);
			glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);

			mProjectM->renderFrame();
		}
	}

	unsigned initRenderToTexture() { return mProjectM->initRenderToTexture(); }
	void changeTextureSize(int size) { mProjectM->changeTextureSize(size); }
	void changePresetDuration(int seconds) { mProjectM->changePresetDuration(seconds); }
	void getMeshSize(int *w, int *h) { mProjectM->getMeshSize(w, h); }

	/// Sets preset iterator position to the passed in index
	void setNextPresetPosition(unsigned int index) { mProjectM->selectPresetPosition(index); }

	/// Plays a preset immediately
	void selectPreset(unsigned int index, bool hardCut) { mProjectM->selectPreset(index, hardCut); }

	/// Removes a preset from the play list. If it is playing then it will continue as normal until
	/// next switch
	void removePreset(unsigned int index) { mProjectM->removePreset(index); }

	/// Removes entire playlist, The currently loaded preset will end up sticking until new presets
	/// are added
	void clearPresetlist() { mProjectM->clearPlaylist(); }

	/// Turn on or off a lock that prevents projectM from switching to another preset
	void setPresetLock(bool isLocked) { mProjectM->setPresetLock(isLocked); }

	/// Returns true if the active preset is locked
	bool isPresetLocked() { return mProjectM->isPresetLocked(); }

	/// Returns index of currently active preset. In the case where the active
	/// preset was removed from the playlist, this function will return the element
	/// before active preset (thus the next in order preset is invariant with respect
	/// to the removal)
	size_t selectedPresetIndex() {
		unsigned int index = 0;

		mProjectM->selectedPresetIndex(index);

		return index;
	}

	/// Returns true if the selected preset position points to an actual preset in the
	/// currently loaded playlist
	bool presetPositionValid() { return mProjectM->presetPositionValid(); }
	bool getErrorLoadingCurrentPreset() { return mProjectM->getErrorLoadingCurrentPreset(); }

	/// Returns the url associated with a preset index
	System::String ^ getPresetURL(size_t index) {
		std::string presetName = mProjectM->getPresetURL(index);
		msclr::interop::marshal_context context;

		return context.marshal_as<System::String ^>(presetName);
	}

	/// Returns the preset name associated with a preset index
	System::String ^ getPresetName(size_t index) {
		std::string presetName = mProjectM->getPresetName(index);
		msclr::interop::marshal_context context;

		return context.marshal_as<System::String ^>(presetName);
	}
	
	/// Add a preset url to the play list. Appended to bottom. Returns index of preset
	size_t addPresetURL(System::String ^ presetURL, System::String ^ presetName) {
		msclr::interop::marshal_context context;
		std::string urlString = context.marshal_as<std::string>(presetURL);
		std::string nameString = context.marshal_as<std::string>(presetName);
		RatingList ratingList = RatingList(TOTAL_RATING_TYPES);

		return mProjectM->addPresetURL(urlString, nameString, ratingList);
	}

	/// Insert a preset url to the play list at the suggested index.
	void insertPresetURL(unsigned int index, System::String ^ presetURL, System::String ^ presetName) {
		msclr::interop::marshal_context context;
		std::string urlString = context.marshal_as<std::string>(presetURL);
		std::string nameString = context.marshal_as<std::string>(presetName);
		RatingList ratingList = RatingList(TOTAL_RATING_TYPES);

		mProjectM->insertPresetURL(index, urlString, nameString, ratingList);
	}

	void changePresetName(unsigned int index, System::String ^ name) {
		msclr::interop::marshal_context context;
		std::string nameString = context.marshal_as<std::string>(name);

		mProjectM->changePresetName(index, nameString);
	}

	/// Returns the rating associated with a preset index
	int getPresetRating(unsigned int index)	{
		return mProjectM->getPresetRating(index, HARD_CUT_RATING_TYPE);
	}

	void changePresetRating(unsigned int index, int rating)	{
		mProjectM->changePresetRating(index, rating, HARD_CUT_RATING_TYPE);
	}

	void showPresetName(bool state) { mProjectM->renderer->showpreset = state; }

	void showFrameRate(bool state) { mProjectM->renderer->showfps = state; }

	/// Returns the size of the play list
	unsigned int getPresetListSize() { return mProjectM->getPlaylistSize(); }

	void evaluateSecondPreset() { mProjectM->evaluateSecondPreset(); }

	void setShuffleEnabled(bool value) { mProjectM->setShuffleEnabled(value); }
	bool isShuffleEnabled() { return mProjectM->isShuffleEnabled(); }

	void selectPrevious(const bool b) { mProjectM->selectPrevious(b); }
	void selectNext(const bool b) { mProjectM->selectNext(b); }
	void selectRandom(const bool b) { mProjectM->selectRandom(b); }

	void addPCMfloat(uint8_t pcmData[], int samples, int channels) {
		if (isInitialized()) { 
			if (channels == 1) {
				mProjectM->pcm()->addPCMfloat((float *)pcmData, samples);
			}
			else {
				mProjectM->pcm()->addPCMfloat_2ch((float *)pcmData, samples);
			}
		}
	}

	int getWindowWidth() { return mProjectM->getWindowWidth(); }
	int getWindowHeight() { return mProjectM->getWindowHeight(); }

	//		PipelineContext &pipelineContext() { return *_pipelineContext; }
	//		PipelineContext &pipelineContext2() { return *_pipelineContext2; }
	//		Pipeline *renderFrameOnlyPass1(Pipeline *pPipeline);
	//		void renderFrameOnlyPass2(Pipeline *pPipeline, int xoffset, int yoffset, int eye);
	//		void renderFrameEndOnSeparatePasses(Pipeline *pPipeline);

	//		void key_handler(projectMEvent event, projectMKeycode keycode, projectMModifier modifier);
	//		void default_key_handler(projectMEvent event, projectMKeycode keycode);

	//		const Settings &settings() const { return _settings; }
	//		static bool writeConfig(const std::string& configFile, const Settings& settings) {}

	/// Sets the randomization functor. If set to null, the traversal will move in order according
	/// to the playlist
	//		void setRandomizer(RandomizerFunctor *functor);

	/// Tell projectM to play a particular preset when it chooses to switch
	/// If the preset is locked the queued item will be not switched to until the lock is released
	/// Subsequent calls to this function effectively nullifies previous calls.
	//	void queuePreset(unsigned int index) { mProjectM->queuePreset(index); } doesn't seem to have
	// an implementation...

	/// Returns true if a preset is queued up to play next
	//	bool isPresetQueued() { return mProjectM->isPresetQueued(); } doesn't seem to have an
	// implementation...
};
