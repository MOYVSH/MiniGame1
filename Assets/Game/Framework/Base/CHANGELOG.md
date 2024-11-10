# Changelog
本文件将记录此软件包的所有重大变更。

格式基于[保持更新日志](http://keepachangelog.com/en/1.0.0/)的规定， 并且遵循[语义化版本控制](http://semver.org/spec/v2.0.0.html)标准。

## [待发布]

优化检查工具窗口的UI布局

## [2.0.0-3.alpha] - 2024-08-12

### 修改

- MagicTavernProfileEvent构造函数更新


## [2.0.0-2.alpha] - 2024-08-12

### 修改

- AssetChecker新增对加载资源的空值判断


## [1.9.9-14.alpha] - 2024-06-19

### 移除

- 重新生成MagicTavernEvent的GUID避免和暂时不升级MagicTavernSystem的工程发生冲突. 
- 移除不必要的资源检查配置属性

## [1.9.9-12.alpha] - 2024-06-19

### 修改

- AssetCheckerData新增属性


## [1.9.9-11.alpha] - 2024-06-11

### 修改

- mt自定义参数持有对象访问级别改为internal

  

## [1.9.9-10.beta] - 2024-06-07

### 修改

- mt消息定义移动到base库

  

## [1.9.9-9.beta] - 2024-05-16



### 修改

- BaseRuler增加权重项



## [1.9.9-8.beta] - 2024-05-16

### 修改

- AssetCheckData挪到ProjectSettings中

  

## [1.9.9-6.beta] - 2024-05-15

### 修复

- 修复图片检查时，加载的资源为空会报错的问题

  

## [1.9.9-4.alpha] - 2024-03-28

### 修改

- 调整AssetChecker库关于audioclip检测的规则
  1. 超过15秒的AudioClip没有设置LoadType为Streaming
  2. AudioClip被错误设置为Force To Mono
  3. 低于15秒的AudioClip是双声道



## [1.9.9-1.alpha] - 2024-03-21

### 新增

- AssetChecker库的类型无关功能移动到当前库, 方便自定义类型扩展资产检查.



## [1.9.6] - 2024-03-01

### 修改

- 整合功能



## [1.9.6] - 2024-03-01

### 新增

- 移入FormatPath方法



## [1.9.5] - 2024-01-26

### 新增

- RuntimeScriptableSingleton



## [1.9.4] - 2023-12-28

### 修改

- SystemLanguage改为LBLaunguage



## [1.9.3-2.alpha] - 2023-11-22

### 新增

- BinaryBaseSerializer



## [1.9.2] - 2024-01-24

### 修复

- 修复EditorScriptableSingleton可能创建失败的bug



## [1.8.8] - 2023-04-07

### 新增

- Test AppID in SettingAsset

  

## [1.8.7] - 2023-01-17

### 新增

- EditorScriptableSingleton

  

## [1.8.4] - 2022-12-28

### 新增

- ScreenCapture



## [1.8.3] - 2022-12-21

### 新增

- LRU class



## [1.8.2] - 2022-11-03

### 修改

- optimize texture extension logic



## [1.8.0] - 2022-10-27

### 修改

- optimize MonoSingleton



## [1.7.8] - 2022-08-10

### 修改

- disable editor singleton reset



## [1.7.5] - 2022-08-04

### 修改

- update ICSharpCode version to 1.2.0



## [1.7.4] - 2022-08-04

### 新增

- argument avoid bleeding

  

## [1.7.0] - 2022-08-02

### 新增

- argument maxValue



## [1.6.27] - 2022-06-07

### 新增

- BaseSettingAsset class

  

## [1.6.21] - 2022-06-07

### 新增

- All clone texture function use LoadImage(originalTexture.EncodeToPNG())

  

## [1.6.18] - 2022-06-07

### 新增

- 创建纹理时禁用mipmaps



## [1.6.16] - 2022-10-09

### 新增

- LBAssetPostprocessor



## [1.6.12] - 2022-10-09

### 新增

- 若干扩展方法



## [1.6.6] - 2022-06-07

### 新增

- Http Uploader



## [1.5.9] - 2022-06-07

### 新增

- 增加图片宽高变为4倍的扩展方法



## [1.5.1] - 2022-06-07

### 新增

- binary serializer



## [1.5.1] - 2022-06-07

### 新增

- 微信发布支持



## [1.4.4] - 2022-06-07

### 修改

- 一些基础类工具类的库



## [1.4.2] - 2022-06-07

### 修改

- AddOnePixelBorder方法增加size参数



## [1.3.8] - 2022-06-07

### 新增

- 图片缩放扩展



## [1.3.7] - 2022-06-07

### 修改

- 更新PNGQuant执行文件



## [1.3.6] - 2022-09-20

### 新增

- Texture Extension 需指定size



## [1.3.4] - 2022-09-20

### 新增

- 部分压缩API



## [1.3.2] - 2022-06-07

### 移除

- 移除对spine的依赖



## [1.1.5] - 2022-10-09

### 新增

- local server support

  

## [1.1.1] - 2022-06-07

### 修改

- StreamingAssets下的settingasset.json改为Build前自动创建，Editor下优先SettingAsset.asset

  

## [1.1.0] - 2022-10-09

### 新增

- SettingAsset to StreamingAsset folder



## [1.0.9] - 2022-06-07

### 增加

- 增加程序集引用



## [1.0.6] - 2022-06-07

### 修复

- 修复ZipHelper里解压文件遇见文件夹报空的Bug



## [1.0.4] - 2022-06-07

### 新增

- 加入对LBLibraryUnitySpine的依赖
- 加入对LBLibraryUnityDOTween的依赖
- 加入对LBLibraryUnitySevenZip的依赖
- 加入对LitJson的依赖
