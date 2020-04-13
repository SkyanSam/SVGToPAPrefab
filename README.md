# SVGToPAPrefab
 Convert SVG files and put them into the game as prefabs.
 
 This is not a full scale SVG conversion program, Don't expect to convert rotated shapes / advanced paths because I haven't created ways to   convert those into basic shapes!
 
 Succesfully Imported: ellipses, squares, circles, triangles, fills, outlines.
  Not Tested: Hexagon
   Not Importable: Rotated Shapes, Advanced Paths
 
 Heres how to setup. I'll include a video relatively soon!
 
 You should only have to touch two classes, UserOptions & Colors
 
 string UserOptions.prefabName < The name of your prefab >
  PrefabType UserOptions.prefabType < The type of your prefab >
   float UserOptions.secondsToLast < How long should the prefab last >
    string UserOptions.svgPath = @"D:/Documents/CSharpProjects/AbandonedBoss.svg" < The location of the svg >
     string UserOptions.prefabPath = @"C:\Program Files (x86)\Steam\steamapps\common\Project Arrhythmia\beatmaps\prefabs" < Your PA prefab folder >
    
string[] Colors.ids < Colors corresponding with each color ID in project arrythmia. This must be edited by the user with the colors they are using in their svg, Max amount is 10 colors >

If you would like to contribute please do. Converting advanced polygons and rotated squares to PA sounds well rough..
 And thanks to TerranByte for the PAPrefabBuilder & Lily for the base game!
        
