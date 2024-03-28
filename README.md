# PLATEAU PCL Generator （3D都市モデルを用いた点群マップの生成システム）

![概要](./markdown/images/intro.png) 

## 1. 概要 
本リポジトリでは、2023年度のProject PLATEAUで開発した「PLATEAU PCL Generator」のソースコードを公開しています。  
「PLATEAU PCL Generator」は、PLATEAUの3D都市モデルを活用し、自律走行車両用の点群マップを生成するためのシステムです。  
本システムでは、3D都市モデルを配置した仮想空間上でLiDARを搭載した車両を走行させ、LiDARの点群放射をシミュレートすることで点群マップを出力することができます。

本システムはUnityのプラグインとして構築され、[PLATEAU SDK for Unity](https://github.com/Project-PLATEAU/PLATEAU-SDK-for-Unity)を前提としています。  
また、本システムによって出力された点群マップはAutowareで利用することが想定されています。

## 2. 「PLATEAU PCL Generator」について
自律走行車両の自己位置測位の仕組みの一つとして、事前に作成された点群マップと車両に取り付けられた3D LiDARの点群データとのマッチングによる位置測位という方法があります。  
この方法により高精度の自己位置を取得することができる反面、点群マップの事前取得が必要となるため、煩雑かつ高コストなプロセスや、走行ルート設定の柔軟性が低いといった点が課題となっています。  
「PLATEAU PCL Generator」は、この課題を解決するため、3D都市モデルを配置した仮想空間内において点群マップを取得するシステムとして開発されました。  
Unityプラグインとして実装することでプロセスの汎用化を図るとともに、仮想空間内の車両や3D LiDARのパラメータ、走行ルート等をGUIから調整可能とすることで、点群生成プロセスの精緻化を実現しています。  

本システムの詳細については[技術検証レポート](https://www.mlit.go.jp/plateau/file/libraries/doc/plateau_tech_doc_0087_ver01.pdf)を参照してください。

![image](https://github.com/Project-PLATEAU/PLATEAU-PointCloud-Generator/assets/79615787/75f33738-989f-4222-9a20-934fc6c2f180)

## 3. 利用手順
本システムの構築手順及び利用手順については[利用チュートリアル](https://project-plateau.github.io/PLATEAU-PointCloud-Generator/)を参照してください。

## 4. システム概要
### ①3D都市モデル配置機能
- PLATEAU SDK for Unityを用いて、3D都市モデルをUnityシーン上に配置します。

### ②車両パラメーター変更機能
- 車両のパラメーターを変更し、形状やLiDARの取り付け位置を変更します。

### ③LiDARパラメーター変更機能
- 仮想LiDARのパラメーターを変更し、照射の飛距離や角度、ステップ数などを変更します。

### ④ルート設定機能
- 道路モデル上にWayPointオブジェクトを配置し、車両が走行するルートを設定します。

### ⑤点群マップ出力機能
- 仮想空間上を車両が走行し、点群マップを出力します。


## 5. 利用技術

| 種別              | 名称   | バージョン | 内容 |
| ----------------- | --------|-------------|-----------------------------|
| ライブラリ       | [PLATEAU SDK for Unity](https://project-plateau.github.io/PLATEAU-SDK-for-Unity/) | 2.3.0 | 3D都市モデルを利用するためのUnityライブラリ |
| ライブラリ      | [RobotecGPULidar](https://github.com/RobotecAI/RobotecGPULidar) | 0.16.2 | Unity上で3D LiDARのシミュレートを行うライブラリ |
| ライブラリ      | [Point Cloud Library](https://github.com/PointCloudLibrary/pcl) | 1.14.0 | RobotecGPULidar内の点群処理で利用されるライブラリ |
| ライブラリ      | [DOTWeen](https://dotween.demigiant.com/) | 1.2.765 | Unityオブジェクトに移動系のアニメーションを付与するライブラリ |


## 6. 動作環境 <!-- 動作環境についての仕様を記載ください。 -->
| 項目               | 最小動作環境                                                                                                                                                                                                                                                                                                                                    | 推奨動作環境                   | 
| ------------------ | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | ------------------------------ | 
| OS                 | Microsoft Windows    　11                                                                                                                                                                                                                                                                                                                  |  同左 | 
| CPU                | Intel Core i7以上                                                                                                                                                                                                                                                                                                                               | 同左              | 
| メモリ             | 8GB以上                                                                                                                                                                                                                                                                                                                                         | 16GB以上                        | 
| ディスプレイ解像度 | 1024×768以上                                                                                                                                                                                                                                                                                                                                    |  同左                   | 

## 7. 本リポジトリのフォルダ構成 <!-- 本GitHub上のソースファイルの構成を記載ください。 -->
| フォルダ名 |　詳細 |
|-|-|
| Assets/HDRP | シーン内で利用する車両オブジェクトが格納されているフォルダ |
| Assets/Images | シーン内で利用する画像ファイルが格納されているフォルダ |
| Assets/Material | シーン内で利用するマテリアルファイルが格納されているフォルダ |
| Assets/PclSharp | RGLUnityPlugin内で点群処理を行うために利用されるPoint Cloud Libraryが格納されているフォルダ |
| Assets/RGLUnityPlugin | RobotecGPULidarのライブラリが格納されているフォルダ |
| Assets/SampleScene | サンプルシーンが格納されているフォルダ |
| Assets/Scripts | シーン上の点群処理に利用するスクリプトが格納されているフォルダ |
| Assets/TextMesh Pro | Unityで利用するテキストのフォーマットやレイアウト処理が格納されているフォルダ |
| Assets/UI | シーン内のUIで利用するコンポーネントが格納されているフォルダ |
| Assets/WaypointSystem | シーン内のルート設定に関わるスクリプトが格納されているフォルダ |
| Packages/ | Unityパッケージのマニフェスト、package-lock.jsonが格納されているフォルダ |
| ProjectSettings/ | Unityプロジェクト設定ファイルが格納されているフォルダ |
| markdown/ | ドキュメントが記載されているフォルダ |


## 8. ライセンス

- ソースコード及び関連ドキュメントの著作権は国土交通省に帰属します。
- 本ドキュメントは[Project PLATEAUのサイトポリシー](https://www.mlit.go.jp/plateau/site-policy/)（CCBY4.0及び政府標準利用規約2.0）に従い提供されています。

## 9. 注意事項

- 本リポジトリは参考資料として提供しているものです。動作保証は行っていません。
- 本リポジトリについては予告なく変更又は削除をする可能性があります。
- 本リポジトリの利用により生じた損失及び損害等について、国土交通省はいかなる責任も負わないものとします。

## 10. 参考資料
- 技術検証レポート: https://www.mlit.go.jp/plateau/file/libraries/doc/plateau_tech_doc_0087_ver01.pdf
- PLATEAU WebサイトのUse Caseページ「3D都市モデルとBIMを活用したモビリティ自律運航システム（車両）v2.0」: https://www.mlit.go.jp/plateau/use-case/uc23-17-2/
