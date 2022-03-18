# GH-Pythonnet

.Net から Python(Python3 以上も含む) を実行できる Pythonnet を使って Grasshopperコンポーネントを作成したリポ

Pythonnet の GitHub リポ

- https://github.com/pythonnet/pythonnet

上記リポの README にも書いてあるが、環境変数 `PYTHONNET_PYDLL` で指定された Python の DLL ファイルを使ってビルドされる。
パッケージもその DLL を使っている Python にインストールされたものが参照可能になっている。

実行内容としては、Pythonnet のサンプルに倣い、Numpy を使って入力値の sin と cos の和を計算するものになっている。
