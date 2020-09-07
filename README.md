# CustomizationInspector
Customize Unity3D inspector by attribute(such as Button, ReadOnly, HideIf etc.) and serializable dictionary.

## Features
* Provide some attributes to change original inspector.(Rename,HideIf,ShowIf,ReadOnly,ValueDropdown,InfoBox,MinMax,Button...)
* Serialize dictionary class to easy edit in inspector.
* Redraw `Vector4` and `Vector4Int` inspector.
* Add Copy and Paste button on `AnimationCurve` inspector.
* An attempt to mimic the ReorderableList within Unity while adding some extended functionality.

## Effect
* CommonAttributes

![image](https://github.com/Mr-sB/CustomizationInspector/blob/master/Screenshots/CommonExample.png)
* MinMax

![image](https://github.com/Mr-sB/CustomizationInspector/blob/master/Screenshots/MinMaxExample.png)
* SerializableDictionary

![image](https://github.com/Mr-sB/CustomizationInspector/blob/master/Screenshots/SerializableDictionaryExample.png)
* AnimationCurve

![image](https://github.com/Mr-sB/CustomizationInspector/blob/master/Screenshots/AnimationCurveExample.png)
* Vector4

![image](https://github.com/Mr-sB/CustomizationInspector/blob/master/Screenshots/Vector4Example.png)
* ReorderableList

![image](https://github.com/Mr-sB/CustomizationInspector/blob/master/Screenshots/ReorderableExample.png)

## Note
`AnimationCurve` require [UnityExtensionUtil](https://github.com/Mr-sB/UnityExtensionUtil) module.

## Thanks
Thanks for cfoulston/Unity-Reorderable-List providing ReorderableList function.
https://github.com/cfoulston/Unity-Reorderable-List
