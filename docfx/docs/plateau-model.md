# 3D都市モデル配置手順

## 1. サンプルシーンをコピー
- サンプルシーンをコピーして、モデル配置用のシーンを作成します。
- 配置済みの3D都市モデルは削除してください。
![plateau-copy](../images/plateau-copy.png) 


## 2. 3D都市モデルを読み込み
- PLATEAU SDK for Unityを利用して、3D都市モデルを読み込みます。
- 3D都市モデルのインポートに関しては、[PLATEAU SDK for Unityのチュートリアル](https://project-plateau.github.io/PLATEAU-SDK-for-Unity/index.html)をご確認ください。
![plateau-sdk](../images/plateau-sdk.png) 



### 注意点： 地物を読み込む際は、`Mesh Colliderをセットする`に必ずチェックを入れてください。
### 橋梁、災害リスク、土地起伏は不要です。
### テクスチャを含めると、シーンのファイルサイズが大きくなるので注意してください。
![plateau-config](../images/plateau-config.png) 

- 神奈川県横浜市みなとみらい地区の3D都市モデルをテクスチャなしで読み込んだ画面
![plateau-load](../images/plateau-load.png) 


