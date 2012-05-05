@MenuItem("Assets/Auto Build Resource Files")
static function ExportResource () {

	var options = BuildAssetBundleOptions.CollectDependencies | BuildAssetBundleOptions.CompleteAssets;
	BuildPipeline.PushAssetDependencies();

	// All subsequent resources share assets in this resource file
	// It is up to you to ensure that the shared resource file is loaded prior to loading other resources
	BuildPipeline.BuildAssetBundle(AssetDatabase.LoadMainAssetAtPath("Assets/artwork/lerpzuv.tif"), null, "Shared.unity3d", options);	

	// By pushing and popping around the resource file, this file will share resources but later resource files will not share assets in this resource
	BuildPipeline.PushAssetDependencies();
	
	BuildPipeline.BuildAssetBundle(AssetDatabase.LoadMainAssetAtPath("Assets/Artwork/Lerpz.fbx"), null, "Lerpz.unity3d", options);	

	BuildPipeline.PopAssetDependencies();

	// By pushing and popping around the resource file, this file will share resources but later resource files will not share assets in this resource
	BuildPipeline.PushAssetDependencies();

	BuildPipeline.BuildAssetBundle(AssetDatabase.LoadMainAssetAtPath("Assets/Artwork/explosive guitex.prefab"), null, "explosive.unity3d", options);	

	BuildPipeline.PopAssetDependencies();

	// By pushing and popping around the resource file, this file will share resources but later resource files will not share assets in this resource
	BuildPipeline.PushAssetDependencies();

	// Build streamed scene file into a seperate unity3d file
	BuildPipeline.BuildPlayer(["Assets/AdditiveScene.unity"], "AdditiveScene.unity3d", BuildTarget.WebPlayer, BuildOptions.BuildAdditionalStreamedScenes);	
	if (System.IO.File.Exists("AdditiveScene.html"))
		System.IO.File.Delete("AdditiveScene.html");	

	BuildPipeline.PopAssetDependencies();
	
	BuildPipeline.PopAssetDependencies();
	
	BuildPipeline.BuildPlayer(["Assets/Loader.unity"], "loader.unity3d", BuildTarget.WebPlayer, BuildOptions.Development);
}