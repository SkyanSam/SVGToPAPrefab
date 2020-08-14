# SVGToPAPrefab
 Convert SVG files to Project Arrythmia prefabs that you can use in levels!
This is in alpha stage, therefore a lot of things aren't implemented so expect errors if you are trying to convert complex stuff (such as paths)
I made a video talking about this project here: 
If you would like to contribute (submit bug / pull request) check out the CONTRIBUTING.md file. https://github.com/SkyanSam/SVGToPAPrefab/blob/master/CONTRIBUTING.md

## Thanks to
Terranbyte for the PAPrefabBuilder https://github.com/Terranbyte/PA-Prefab-Builder
Lily for the base game

## Current State
	Succesfully Imported: ellipses, squares, circles, triangles, fills, outlines.
	Not Tested: hexagon

# Documentation

A lot of the documentation is in <summary></summary> above the various classes/functions
ConsoleApp1.proj contains the actual converter
UIWPF.proj is just an addon to ConsoleApp1 as the user interface.
Input.cs contains all the input variables
GameObjectData.cs is intended to hold classes that will be compiled to GameObjects later on {GameObjectData, PathOutline, 
DataAppliers.cs is intended to apply attribute values to GameObjectDatas {@d, @x, @y, @width, etc.. } 
LineWriter.cs is used for debugging
You may also use the PAPrefabBuilders documentation for classes such as GameObject.

## Input Class
You should only need to edit Input if you want to change the default values & you aren't planning on using UIWPF, however you can change the values in the UI.
 
### string Input.prefabName 
// The name of your prefab
Example: Nowo123
  PrefabType UserOptions.prefabType < The type of your prefab >
  
### float Input.secondsToLast 
// How long should the prefab last
Example: 1
   
### string Input.svgPath 
// The location of the svg
Example: @"D:/Documents/Nowo.svg" 

### string UserOptions.prefabPath 
// Your Project Arrythmia prefab folder
Example: @"C:\Program Files (x86)\Steam\steamapps\common\Project Arrhythmia\beatmaps\prefabs" 
    
### string[] Colors.ids 
// Colors corresponding with each color ID in project arrythmia. This must be edited by the user with the colors they are using in their svg, Max amount is 10 colors


        
