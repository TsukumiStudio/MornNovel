# MornNovel

## 概要

ノベルゲーム/ビジュアルノベルエンジン用のシステムライブラリ。ノベル再生、キャラクター表示、メッセージ表示、背景切り替え、BGM・SE管理を統合的に提供。

## 依存関係

| 種別 | 名前 |
|------|------|
| 外部パッケージ | Arbor, UniTask, VContainer, UniRx, TextMesh Pro, Cinemachine（オプション）, Addressables（オプション） |
| Mornライブラリ | MornArbor, MornBeat, MornSound, MornDebug, MornTransition, MornLocalize, MornScene, MornColor, MornGlobal, MornUtil, MornTween, MornUGUI |

## 使い方

### セットアップ

1. `MornNovelGlobal` でノベルシーン、アドレス設定を定義
2. `MornNovelSettings` でBGMフェード時間、キャラ移動設定などを調整
3. `MornNovelLifetimeScope` でVContainer DI設定

### ノベル再生

```csharp
MornNovelService.SetNovelAddress();
// → ノベルシーン読み込み
// → Arborコマンドで順序制御
```

### 主要なArborコマンド

| コマンド | 機能 |
|---------|------|
| MornNovelStartCommand | ノベル開始 |
| MornNovelMessageCommand | セリフ表示 |
| MornNovelCharaShowCommand | キャラ登場 |
| MornNovelSetBackgroundCommand | 背景設定 |
| MornNovelBgmCommand | BGM再生/停止 |
| MornNovelCameraShakeCommand | カメラ揺れ |
| MornNovelEndCommand | ノベル終了 |

### 既読管理

```csharp
// 既読判定
bool isRead = MornNovelService.IsNovelRead();

// 既読フラグ登録
MornNovelService.AtNovelReadEnd();
```
