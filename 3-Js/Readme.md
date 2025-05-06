# JavaScriptの基礎

JavaScriptの基礎的知識について記載します。

## 目次
- [JavaScriptの基礎](#javascriptの基礎)
  - [目次](#目次)
  - [JavaScriptとは何か](#javascriptとは何か)
  - [ECMAScript](#ecmascript)
  - [どこにJavaScriptを記載するのか](#どこにjavascriptを記載するのか)
    - [HTMLファイル内での直接記述](#htmlファイル内での直接記述)
    - [外部JavaScriptファイルの参照](#外部javascriptファイルの参照)
    - [HTML要素の属性として](#html要素の属性として)
    - [`<script>`タグはどこに書くべきか](#scriptタグはどこに書くべきか)
      - [時代によって異なる](#時代によって異なる)
      - [現代の回避策](#現代の回避策)
  - [JavaScriptの書き方](#javascriptの書き方)
    - [変数の宣言方法](#変数の宣言方法)
    - [データ型について](#データ型について)
      - [動的型付け、非TypeSafeの言語である](#動的型付け非typesafeの言語である)
      - [主要なデータ型](#主要なデータ型)
      - [変数の型を確認するには](#変数の型を確認するには)
    - [関数とメソッド](#関数とメソッド)
      - [関数 (Function)](#関数-function)
      - [メソッド (Method)](#メソッド-method)
      - [アロー関数](#アロー関数)
      - [主な違い](#主な違い)
    - [class](#class)
      - [コンストラクタ](#コンストラクタ)
      - [プロパティ](#プロパティ)
      - [アクセシビリティ（アクセス修飾子）](#アクセシビリティアクセス修飾子)
      - [継承](#継承)
      - [静的メソッド](#静的メソッド)
    - [条件分岐と繰り返し](#条件分岐と繰り返し)
    - [演算子](#演算子)
      - [厳密等価比較（`===`）](#厳密等価比較)
      - [厳密不等価比較（`!==`）](#厳密不等価比較)
  - [JavaScriptとHTML](#javascriptとhtml)
    - [HTML要素を取得する](#html要素を取得する)
    - [HTML要素を操作する](#html要素を操作する)
    - [JavaScriptを実行する](#javascriptを実行する)
    - [HTML要素のイベント](#html要素のイベント)
      - [マウスイベント](#マウスイベント)
      - [キーボードイベント](#キーボードイベント)
      - [フォームイベント](#フォームイベント)
      - [ウィンドウイベント](#ウィンドウイベント)
      - [その他のイベント](#その他のイベント)
  - [イベント処理の注意点](#イベント処理の注意点)
    - [イベントのキャンセル](#イベントのキャンセル)
    - [イベントバブリングとキャプチャリング](#イベントバブリングとキャプチャリング)
      - [イベントバブリング](#イベントバブリング)
      - [イベントキャプチャリング](#イベントキャプチャリング)
      - [問題になるケース](#問題になるケース)
      - [回避方法](#回避方法)

## JavaScriptとは何か

JavaScriptは、Webページに動的な機能を追加するために使用されるプログラミング言語です。  
以下がJavaScriptの主な特徴です。

- Webブラウザで実行される最も一般的なスクリプト言語
- HTML/CSSと組み合わせて使用され、インタラクティブなWebフロントを構築できる
- 動的なコンテンツの更新、アニメーション、データの処理などが可能
- クライアントサイドだけでなく、Node.js環境下であればサーバーサイドでも動作できる

現代のWeb開発において、JavaScriptは不可欠な技術となっています。

最近ではTypeScriptの一般化によって、JavaScriptは「Vanilla JS」や「Pure JavaScript」と呼称され、TypeScriptと区別されます。

## ECMAScript

Styleの章でも触れましたが、ECMAScript（エクマスクリプト）は、JavaScriptの標準仕様を定めたスクリプト言語の規格です。  
JavaScriptはこのECMAScriptを基に実装されており、以下のような特徴があります。

- 標準化: ECMA Internationalによって標準化されている。
- バージョン管理: ECMAScriptは定期的に新しいバージョンがリリースされ、言語機能が拡張されている。
  - 例: ES5、ES6（ES2015）、ES7（ES2016）など。
- 互換性: 新しいバージョンでも後方互換性を保つよう設計されている。

ECMAScriptの進化により、JavaScriptはより強力で柔軟な言語となり、モダンなWeb開発において重要な役割を果たしています。

ECMAScriptのバージョンは、シンプルに`.cshtml`や`.js`ファイルにJavaScriptコードを記載して実行する場合、特にバージョン指定は不要です。
本稿では触れませんが、TypeScriptを使用する場合はTypeScript⇒JavaScriptへ変換しないとブラウザが解釈できませんので、"どのECMAScriptバージョンで変換するか"の指定が必要になります。

そのバージョン指定で"ES6"や"ES2015"といった単語が出てきます。
ここでは、"ES"とは"ECMAScript"を意味している、という事だけ覚えておいてください。

## どこにJavaScriptを記載するのか

JavaScriptコードは主に以下の3つの方法で記載することができます。

### HTMLファイル内での直接記述

`<script>`タグを作成し、その中に記載します。
```html
<script>
    // JavaScriptコードをここに記述
    window.alert("Hello, World!");
</script>
```

このケースは[こちらのサンプル](./js-sample1-innerscript.html)になります。  
記述したJavaScript処理が有効なのはHTMLファイル内に限りますので、コード管理と汎用性に課題があります。

また、HTMLはデザインを決める役割、JavaScriptはコード処理の役割を負います。  
つまり、JavaScript処理をHTMLに記述することはソフトウェア設計の原則のうちの「関心事の分離」に反します。

### 外部JavaScriptファイルの参照

`.js`ファイルを作成し、そのファイルをhtmlから`<script>`タグで読み込みます。
```html
<script src="path/to/script.js"></script>
```

このケースは[こちらのサンプル](./js-sample2-externalfile.html)になります。  
分離することで、上記の「HTMLファイル内での直接記述」に記載したネガティブを回避できます。　　
更には、CSS外部ファイルと同じようにブラウザ（またはCDN）キャッシュを活用できます。

### HTML要素の属性として

このパターンを使う事はまずありませんが、一応紹介しておきます。  
HTML要素のイベント属性(`onclick`や`onblur`など)にJavaScript処理を直接記入できます。
```html
<button onclick="window.alert('クリックされました')">クリックしてください</button>
```

### `<script>`タグはどこに書くべきか

#### 時代によって異なる

これは時代によって異なります。  
かつてはHTMLの`<head>`タグ内に`<script>`タグを記載するのは回避した方がよいとされていました。

その理由としては、ブラウザの処理順にあります。  
ブラウザは、`<head>`⇒`<body>`の順に処理していきますが、これらは同期的に処理されていました。  
JavaScriptを`<head>`に記載すると、`.js`ファイルのダウンロードや実行が完了するまで、ブラウザは次の処理に進みません。  
画面デザインは`<body>`に記載されていますから、JavaScriptの処理が完了するまでブラウザは画面表示まで到達できない、つまり処理が遅延するという事です。

[このサンプル](./js-sample3-oldstyle.html)をブラウザで表示してみてください。  
`window.alert();`を`<head>`タグ内で呼び出していますが、alertダイアログ表示中はJavaScriptの実行を中断する（=ブラウザのJavaScript処理が終了しない）ため、この間ブラウザには白い画面が表示されます。

よって、かつてはbodyの閉じタグ（`</body>`）の直前に記述するのが推奨されていました。 
製造時期の古いものは、以下のように記載されているものが多くあります。

```html
<!DOCTYPE html>
<html>
<head>
    <title>ページタイトル</title>
</head>
<body>
    <!-- HTMLコンテンツ -->
    
    <!-- JavaScriptの読み込み -->
    <script src="script.js"></script>
</body>
</html>
```

#### 現代の回避策

上述のような`script`タグの問題点を解決するため、現代では下記の属性やアプローチが導入されました。

1. **async属性**
   - スクリプトの読み込みをHTMLの解析と並行して行います
   - ダウンロードが完了次第実行されます
     - `<body>`タグ内の解析完了は待機しませんので、JavaScript処理でHTML上の要素に影響するような処理（例えば表示/非表示切替）は動作保証できません
   - 実行順序は保証されません
     - つまり下記の`script1.js`と`script2.js`、どちらが先に実行されるのかは保証されません
   ```html
    <head>
      <script async src="script1.js"></script>
      <script async src="script2.js"></script>
    </head>
   ```

2. **defer属性**
   - スクリプトの読み込みをHTMLの解析と並行して行います
   - HTMLの解析が完了してから実行されます
   - 記述順序通りに実行されることが保証されます
     - つまり下記の場合、`script1.js`⇒`script2.js`の順に実行されます
   ```html
    <head>
      <script defer src="script1.js"></script>
      <script defer src="script2.js"></script>
    </head>
   ```

3. **type="module"**
   - JavaScriptをモジュールとして扱います
     - モジュールについては本稿では解説しません  
     詳細は以下のページを参照してください
        - [MDNのモジュールに関する解説](https://developer.mozilla.org/ja/docs/Web/JavaScript/Guide/Modules)
   - デフォルトで`defer`と同様の動作をします
   - モジュールスコープが提供され、グローバルスコープの汚染を防ぎます
   ```html
   <script type="module" src="module1.js"></script>
   <script type="module" src="module2.js"></script>
   ```

これらの属性を使用することで、`head`タグ内にスクリプトを配置しても、
- HTMLの解析がブロックされない
- 実行タイミングを制御できる
- パフォーマンスが向上する

という利点が得られ、以前のような配置に関する懸念は大幅に軽減されました。

では、それぞれどのようなケースで採用するべきかという悩みも発生します。  
それについて、以下に纏めます。

| 属性 | 使用するべきパターン |
|------|-------------------|
| async | - JavaScriptが互いに依存関係がない場合<br>- Google Analyticsなどの外部トラッキングスクリプト<br>- 広告スクリプトなど、ページ表示と独立して動作するもの |
| defer | - HTML要素を操作する処理<br>- 他のJavaScriptファイルや`<script>`ブロックに依存するスクリプト<br>- 実行順序が重要であるもの<br>- アプリケーションの主要な処理を記載したもの |
| module | - ESモジュールを使用するモダンなアプリケーション<br>- コードを分割して管理する必要がある場合<br>- プライベートスコープが必要な場合<br>- import/exportを使用する場合 |

基本的には`defer`を使用し、特殊なケースで`async`や`module`を選択することをお勧めします。

## JavaScriptの書き方

### 変数の宣言方法

JavaScriptでの変数宣言には、主に3つの方法があります。

1. **let**: ブロックスコープの変数宣言
  ```javascript
  let name = "John";
  let age = 25;
  ```
  - 再宣言不可
  - 再代入可能
  - ブロックスコープ（`{}`内でのみ有効）

2. **const**: 定数の宣言
  ```javascript
  const PI = 3.14;
  const MAX_COUNT = 100;
  ```
  - 再宣言不可
  - 再代入不可
  - ブロックスコープ

3. **var**: 関数スコープの変数宣言（非推奨）
  ```javascript
  var message = "Hello";
  ```
  - 再宣言可能
  - 再代入可能
  - 関数スコープ
  - [ホイスティング](https://qiita.com/TakanoriOkawa/items/ab51d9bf33cbd2f82501)の影響を受ける
    - 変数が(コード上)宣言されていないにも関わらず、エラーにならない事象を有む
    - 変数の格納値が`undefined`(未定義)

モダンなJavaScriptでは、基本的に`let`と`const`を使用し、`var`は避けることが推奨されています。  
変数の値を変更する必要がない場合は`const`を、変更が必要な場合は`let`を使用します。

### データ型について

#### 動的型付け、非TypeSafeの言語である

JavaScriptは動的型付け言語です。  
これは、変数の型を明示的に宣言する必要がなく、値を代入した時点で自動的に型が決定されることを意味します。

しかしこれは、以下のような記述を許可するという意味でもあります。

``` javascript
let foo = 42; // 宣言時にintを代入することで、fooはint型になる ⇒ 動的型付けである
foo = "bar"; // stringを代入してstring型になった ⇒ int型変数にstringを代入できる、つまりTypeSafeではない
foo = true; // booleanを代入してboolean型になった
```

JavaScriptは、上記のような実装をエラーにしません。  
つまり、実装時にエンジニアは「変数に何の型のデータが入っているか」という点について常に疑問を持たなければならない、という事になります。

これは混乱を生みます。  
特に変数の宣言と、その値を使用・代入するコードが離れた場所に記載している場合は追跡が難しくなります。  
だからこそ、スコープ範囲の狭い`let`を使用し、`var`の利用は避けてください。

また、JavaScriptは暗黙の型変換（型強制）を行います。これは時として予期せぬ結果を引き起こす可能性があります。

```javascript
// 文字列と数値の加算
console.log("5" + 3);     // "53" (数値が文字列に変換される)
console.log("5" - 3);     // 2 (文字列が数値に変換される)

// 真偽値との演算
console.log(true + 1);    // 2 (trueは1に変換される)
console.log(false + 1);   // 1 (falseは0に変換される)

// 等価演算子での比較
console.log(5 == "5");    // true (型変換が行われる)
console.log(5 === "5");   // false (型変換が行われない)
```

予期せぬ動作を防ぐため、厳密等価演算子（`===`）の使用や、明示的な型変換を行うことが推奨されます。

#### 主要なデータ型

1. **プリミティブ型**（基本データ型）
  - number: 数値（整数と浮動小数点数）
    ```javascript
    let age = 25;        // 整数
    let price = 19.99;   // 浮動小数点数
    ```
  - string: 文字列
    ```javascript
    let name = "John";
    let message = 'Hello';  // シングルクォートも可
    ```
  - boolean: 真偽値
    ```javascript
    let isActive = true;
    let isLoggedIn = false;
    ```
  - null: 値が存在しないことを示す
    ```javascript
    let empty = null;
    ```
  - undefined: 値が未定義であることを示す
    ```javascript
    let notDefined;  // 自動的にundefinedになる
    ```

2. **オブジェクト型**
  - object: キーと値のペアを持つ集合
    ```javascript
    let person = {
     name: "John",
     age: 25,
     someMethod() {
      // メソッドの処理を記述
     }
    };
    ```
  - array: 配列
    ```javascript
    let colors = ["red", "green", "blue"];
    ```

#### 変数の型を確認するには

型の確認は`typeof`演算子を使用できます。
```javascript
console.log(typeof 42);          // "number"
console.log(typeof "Hello");     // "string"
console.log(typeof true);        // "boolean"
console.log(typeof [1, 2, 3]);   // "object"
```

### 関数とメソッド

JavaScriptにおいて、関数とメソッドは似たような概念ですが、厳密には異なるものです。  

#### 関数 (Function)
関数は、特定の処理を実行するために定義された独立したコードのブロックです。  
関数はグローバルスコープまたはローカルスコープで定義され、オブジェクトに属していない場合に使用されます。

```javascript
function greet(name) {
  return `Hello, ${name}!`;
}

console.log(greet("Alice")); // "Hello, Alice!"
```

これをC#にたとえるのは難しいですが、`class`や`struct`に属さない、グローバルにどこからでも呼び出せるstaticメソッドと考えてください。  
但し、JavaScriptが実行されるのはあくまでブラウザ上ですから、関数が記載されたファイルが読み込まれていなければ呼び出すことはできません。

#### メソッド (Method)
メソッドは、オブジェクトやクラスのプロパティとして定義された関数です。C#のメソッドと同じものという解釈でOKです。  
メソッドは、そのオブジェクトやクラスに関連する動作を定義します。

```javascript
const person = {
  name: "Bob",
  greet() {
    return `Hello, my name is ${this.name}.`;
  }
};

console.log(person.greet()); // "Hello, my name is Bob."
```

#### アロー関数

アロー関数はFunctionの1種で、ES6（ECMAScript 2015）で導入された簡潔な関数の記述方法です。  
従来の`function`キーワードを使用した関数表記に比べて、コードを短く記述できます。

```javascript
// 従来の関数表記
const add = function(a, b) {
  return a + b;
};

// アロー関数表記
const add = (a, b) => a + b;
```

特徴としては以下の通りです

- 簡潔に記載できる
- 通常の関数(function)とは`this`のスコープが異なる
  - functionの場合はグローバルスコープ、ラムダ関数は呼び出し元
  - [この解説](https://qiita.com/mejileben/items/69e5facdb60781927929)が参考になります
- コンストラクタ(後述)としては使用できない

アロー関数は無名関数を宣言する際に特に利便性が高いです。  
そのため、コールバック関数や短い関数を記述する際に便利です。

#### 主な違い

| 観点       | 関数 (Function)                              | アロー関数 (Arrow Function)                     | メソッド (Method)                              |
|------------|---------------------------------------------|------------------------------------------------|-----------------------------------------------|
| 所属       | 独立して存在し、オブジェクトに属さない       | 独立して存在し、オブジェクトに属さない         | オブジェクトやクラスに属する                   |
| 書き方     | `function`キーワードを使用                  | `=>` を使用                                    | オブジェクトやクラス内で定義                   |
| 呼び出し方 | 関数名で直接呼び出し                        | 関数名で直接呼び出し                           | オブジェクトやクラスのプロパティとして呼び出し |
| thisの挙動 | 呼び出し元によって動的に決定される          | 定義されたスコープを保持（親スコープに束縛）   | 呼び出し元のオブジェクトを参照                 |

### class
JavaScriptでは、C#と同じように`class`を使用してオブジェクト指向プログラミングを行うことができます。  
`class`はES6（ECMAScript 2015）で導入され、オブジェクトの作成や継承を簡潔に記述できるようになりました。

```javascript
class Person {
  // コンストラクタ
  constructor(name, age) {
    this.name = name; // プロパティの定義
    this.age = age;
  }

  // メソッドの定義
  greet() {
    console.log(`Hello, my name is ${this.name} and I am ${this.age} years old.`);
  }
}

// インスタンスの生成
const john = new Person("John", 25);
john.greet(); // "Hello, my name is John and I am 25 years old."
```

#### コンストラクタ

コンストラクタの書き方は、C#とは異なりクラス名ではありません。  
`constructor`メソッドがコンストラクタです。

#### プロパティ

これもC#とは異なります。  
`this`キーワードを使用してプロパティを定義します。

#### アクセシビリティ（アクセス修飾子）

上記サンプルコードを見ると、`public`や`private`といったアクセスレベルを制御するキーワードが無い事に気付くと思います。  
JavaScriptでは、この制御は(今のところ)一般的ではありません。  
すべて`public`となります。

但し、ES2022でPrivate Fieldが導入されました。  
https://qiita.com/rana_kualu/items/8bafecd760ae69cfac41#class-fields

今後普及していく事が予想されていますので、覚えておくとよいでしょう。

#### 継承
`extends`キーワードを使用してクラスを継承できます。

```javascript
// 動物クラス
class Animal {
  constructor(name) {
    this.name = name;
  }

  speak() {
    console.log(`${this.name} makes a noise.`);
  }
}

// 犬クラス(Animalの派生クラス)
class Dog extends Animal {
  speak() {
    console.log(`${this.name} barks.`);
  }
}

const dog = new Dog("Rex");
dog.speak(); // "Rex barks."
```

#### 静的メソッド
`static`キーワードを使用して、インスタンス化せずに呼び出せるメソッドを定義できます。

```javascript
class MathUtils {
  static add(a, b) {
    return a + b;
  }
}

console.log(MathUtils.add(2, 3)); // 5
```

### 条件分岐と繰り返し

JavaScriptにおいて、以下のような条件分岐処理や繰り返し処理はほぼC#のような言語と変わりありません。

- 条件分岐
  - `if ... else`
  - `switch`
  - 三項演算
- 繰り返し
  - `for`
  - `do ... while`
  - `while`

特徴的、かつよく使用するものをピックアップします。

1. `for ... of`
配列のような反復可能なオブジェクトに対し、その要素を先頭から1件ずつ処理します。  
C#でいうところの`foreach`に該当します。

``` javascript
const array1 = ["a", "b", "c"];

for (const element of array1) {
  console.log(element);
}

// Expected output: "a"
// Expected output: "b"
// Expected output: "c"
```

2. `for ... in`
オブジェクトの列挙可能プロパティすべてに対して繰り返しを処理を行います。

``` javascript
const object = { a: 1, b: 2, c: 3 };

for (const property in object) {
  console.log(`${property}: ${object[property]}`);
}

// Expected output:
// "a: 1"
// "b: 2"
// "c: 3"
```

C#でこれを完全に代替する文法はありませんが、しいて言えば`Dictionary.Keys`に対して`foreach`で全要素を処理する、というのがイメージとしては近いでしょう。

### 演算子

演算子に関しても一般的言語のそれとほぼ同じです。  
但し明確に特徴を持つポイントとして、`===`による厳密等価比較があります。

#### 厳密等価比較（`===`）

`===`はJavaScriptにおける厳密等価比較演算子です。この演算子は、値だけでなく型も比較するため、より厳密な比較を行います。

1. **型の一致を確認**  
  `===`は、比較する2つの値が同じ型である場合にのみ`true`を返します。
  ```javascript
  console.log(5 === 5);       // true (値と型が一致)
  console.log(5 === "5");     // false (値は一致するが型が異なる)
  ```

2. **暗黙の型変換を行わない**  
  `==`（等価演算子）は暗黙の型変換を行いますが、`===`は行いません。
  ```javascript
  console.log(5 == "5");      // true (型変換が行われる)
  console.log(5 === "5");     // false (型変換が行われない)
  ```

3. **信頼性の向上**  
  暗黙の型変換による予期しない動作を防ぐため、`===`を使用することが推奨されます。

```javascript
// 数値の比較
let a = 10;
let b = "10";

console.log(a == b);  // true (型変換が行われる)
console.log(a === b); // false (型変換が行われない)

// オブジェクトの比較
let obj1 = { key: "value" };
let obj2 = { key: "value" };

console.log(obj1 == obj2);  // false (異なるオブジェクト参照)
console.log(obj1 === obj2); // false (異なるオブジェクト参照)
```

- `===`は型の一致も確認するため、比較対象の型が異なる場合は必ず`false`になります。
- 配列やオブジェクトの比較では、参照先が同じかどうかを確認します。内容が同じでも異なる参照であれば`false`になります。

JavaScriptで値の比較を行いたい場合、`===`を使用してください。
`===`を使用することで、より厳密で予測可能なコードを記述できます。特に型の一致が重要な場合や、暗黙の型変換を避けたい場合に有効です。

逆に`==`を使用するケースは稀で、禁止に近い非推奨と考えてください。  
Lintのルールセットとしても、エラーとなるのが一般的です。

`==`を使用するのは、比較対象の経緯が追跡しきれず型変換をすると予測できない影響がある場合など、特別な事情がある場合に限ります。  
あくまで致し方ない消極的選択、問題の回避で使用するものです。

#### 厳密不等価比較（`!==`）

厳密等価比較の否定形です。  
従って、不等価比較`!=`も存在します。

特徴や使用推奨についても、`===`のケースと同じです。  
`!=`は使用しないようにしてください。

## JavaScriptとHTML

JavaScriptは、HTML要素を変更したり、逆にHTML要素からJavaScriptの関数・メソッドを呼び出すことができます。

例えば、チェックボックスがチェックされた場合だけボタンを表示したいと考えるとします。
Webアプリケーションはサーバから応答されたHTMLをブラウザ上で表示するものですので、チェックがOn/Offに変更された事をトリガにして、サーバ側に新しいHTMLを要求します。  
ブラウザは、サーバからの応答を基に画面描画を更新します。

しかしこれは、サーバとのHTTP通信が必要になりますので処理に時間が掛かりますし、何より実装が面倒です。  

現在クライアント側にダウンロードされている情報だけで処理目的が達成できるのならば、わざわざサーバに通信する必要はありません。  
そういったケースでは、JavaScript処理だけで完結するべきです。

### HTML要素を取得する

まず、JavaScriptコードからHTMLの要素を取得する方法について記載します。

1. **getElementById**
```javascript
// 指定したIDを持つ要素を取得
const element = document.getElementById("myElement");
```
最も古典的な方法です。  
上記サンプルでは、`<button id="myElement"></button>`のように、id属性に対して完全一致検索します。

しかし、id属性は先述の通りモダンなWeb開発ではあまり使用しません。

2. **getElementsByClassName**
```javascript
// 指定したクラスを持つ要素をHTMLCollectionとして取得
const elements = document.getElementsByClassName("myClass");
```
class属性に対して検索します。  
HTML要素に`class="myClass"`を **含む** 要素が抽出されます。 
但し部分一致検索ではありませんので、 `class="myClass1"`のようなパターンには一致しません。

以下のように、複数のclass名を指定してAnd検索することもできます。
``` javascript
// myClassとanotherClassの両方が設定されている要素を取得
const elements = document.getElementsByClassName("myClass anotherClass");
```
この場合、`class="myClass anotherClass"`のように、引数に指定したclass名が両方ともヒットする要素を抽出します。  
`class="myClass anotherClass someClass"`のようなパターンでもOKです。  
AND検索ですので、`class="myClass someClass"`であれば`anotherClass`が設定されていませんから、これはヒットしません。

`getElementById`とは異なり、複数のHTML要素に同じclassを設定することが可能です。  
そのため取得結果は`HTMLCollection`、つまり配列です。

3. **querySelector**
```javascript
// CSS セレクタにマッチする最初の要素を取得
const element = document.querySelector(".myClass input[type='text']");
```
CSSセレクタを抽出条件に指定できます。  
これにより、非常に柔軟な抽出が可能です。

但し、この方法で取得されるのは最初の要素です。  
[:first-of-type](https://developer.mozilla.org/ja/docs/Web/CSS/:first-of-type)サブクラスが無条件で適用される、と覚えればよいでしょう。

4. **querySelectorAll**
```javascript
// CSS セレクタにマッチする全ての要素をNodeListとして取得
const elements = document.querySelectorAll(".myClass input[type='text']");
```
`querySelector`の複数要素版です。  
こちらの方法では`:first-of-type`が適用されず、ヒットした複数の要素が取得されます。

複数処理したいのか、それとも1要素だけに処理をしたいのか、それによって`querySelector`と`querySelectorAll`を使い分けてください。

### HTML要素を操作する

取得した要素は以下のように操作できます。
```javascript
// 要素の内容を変更
element.textContent = "新しいテキスト";

// クラスの追加/削除
element.classList.add("newClass");
element.classList.remove("oldClass");

// スタイルの変更
element.style.display = "none";

// 属性の変更
element.setAttribute("disabled", "true");
```

### JavaScriptを実行する

HTML要素からJavaScript処理を実行する方法は、主に以下の3つがあります。

1. **HTML属性でイベントを直接指定（非推奨）**
```html
// HTMLコードに直接呼出しを記載
<button onclick="doSomething()">クリック</button>
```
この方法は追跡性は高いですが、古典的な手法で推奨されません。  
`onclick`の中身はjavaScript処理ですから、上述の「HTMLとJavaScriptの関心事は分離するべき」という設計原則に反します。

2. **HTML要素のプロパティに関数を設定**
```javascript
// JavaScript
document.getElementById("myButton").onclick = function() {
  // 処理を記述
};
```

3. **addEventListener メソッドを使用（推奨）**
```javascript
// JavaScript
document.getElementById("myButton").addEventListener("click", function() {
  // 処理を記述
});
```

`addEventListener`は以下の理由で最も推奨される方法です。
- 同一イベントに複数のハンドラを追加できる
- イベントの伝播（バブリングとキャプチャリング）を制御できる。これが最大の理由。
  - これについては後述
- `removeEventListener`で後からハンドラを削除できる

### HTML要素のイベント

以下は、JavaScriptでよく使用されるHTMLイベントの一覧です。

#### マウスイベント
- `click`: 要素がクリックされたときに発生
- `dblclick`: 要素がダブルクリックされたときに発生
- `mousedown`: マウスボタンが押されたときに発生
- `mouseup`: マウスボタンが離されたときに発生
- `mousemove`: マウスカーソルが要素上で移動したときに発生
- `mouseenter`: マウスカーソルが要素に入ったときに発生
- `mouseleave`: マウスカーソルが要素から離れたときに発生

#### キーボードイベント
- `keydown`: キーが押されたときに発生
- `keyup`: キーが離されたときに発生

#### フォームイベント
- `submit`: フォームが送信されたときに発生
- `change`: 入力要素の値が変更されたときに発生
- `input`: 入力要素の値が変更されたときにリアルタイムで発生
- `focus`: 要素がフォーカスされたときに発生
- `blur`: 要素のフォーカスが外れたときに発生

#### ウィンドウイベント
- `load`: ページが完全に読み込まれたときに発生
- `resize`: ウィンドウサイズが変更されたときに発生
- `scroll`: ページがスクロールされたときに発生

#### その他のイベント
- `contextmenu`: 右クリックメニューが表示されたときに発生
- `wheel`: マウスホイールが操作されたときに発生

これらのイベントは、`addEventListener`メソッドを使用して要素にバインドすることで利用できます。
```javascript
element.addEventListener('click', function(event) {
  console.log('クリックされました');
});
```

## イベント処理の注意点

HTMLからJavaScript処理を呼び出すことでイベント処理を実装できますが、幾つか注意が必要です。

### イベントのキャンセル

イベントのキャンセルは、特定のイベントが発生した際にそのデフォルトの動作を抑制するために使用されます。  
例えば、リンクをクリックした際にページ遷移を防ぐ、フォームの送信をキャンセルするなどのケースで利用されます。

JavaScriptでは、`preventDefault()`メソッドを使用してイベントをキャンセルできます。  
以下はその使用例です。

1. **リンクのデフォルト動作をキャンセル**
```javascript
document.querySelector("a").addEventListener("click", function(event) {
  event.preventDefault(); // ページ遷移を防ぐ
  console.log("リンクがクリックされましたが、ページ遷移はキャンセルされました。");
});
```

2. **フォーム送信のキャンセル**
```javascript
document.querySelector("form").addEventListener("submit", function(event) {
  event.preventDefault(); // フォーム送信をキャンセル
  console.log("フォーム送信がキャンセルされました。");
});
```

- `preventDefault()`は、イベントがキャンセル可能な場合にのみ効果を発揮します。  
  例えば、`click`イベントや`submit`イベントはキャンセル可能ですが、`load`イベントなどはキャンセルできません。
- イベントがキャンセル可能かどうかは、`event.cancelable`プロパティで確認できます。

### イベントバブリングとキャプチャリング

JavaScriptでは、イベントが発生した際に、そのイベントがどのように伝播するかを制御する仕組みとして「イベントバブリング」と「イベントキャプチャリング」があります。

#### イベントバブリング
イベントバブリングとは、イベントが発生した要素から親要素へと順に伝播していく仕組みです。  
例えば、子要素をクリックした場合、そのクリックイベントは子要素で処理された後、親要素、さらにその親要素へと伝播します。

```html
<div id="parent">
  <button id="child">クリック</button>
</div>
```

```javascript
document.getElementById("parent").addEventListener("click", function() {
  console.log("親要素がクリックされました");
});

document.getElementById("child").addEventListener("click", function() {
  console.log("子要素がクリックされました");
});
```

上記のコードでは、`child`ボタンをクリックすると以下の順序でログが出力されます。
```
子要素がクリックされました
親要素がクリックされました
```

#### イベントキャプチャリング
イベントキャプチャリングとは、イベントが親要素から子要素へと順に伝播していく仕組みです。  
つまり、イベントが最初に親要素で処理され、その後子要素へと伝播します。

キャプチャリングを有効にするには、`addEventListener`の第3引数に`true`を指定します。

```javascript
document.getElementById("parent").addEventListener("click", function() {
  console.log("親要素がクリックされました");
}, true);

document.getElementById("child").addEventListener("click", function() {
  console.log("子要素がクリックされました");
}, true);
```

この場合、`child`ボタンをクリックすると以下の順序でログが出力されます。
```
親要素がクリックされました
子要素がクリックされました
```

#### 問題になるケース
1. **意図しないイベントの発火**
   - 子要素で処理したイベントが親要素にも伝播してしまい、意図しない動作を引き起こすことがあります。
   - 例: 子要素のクリックイベントで特定の処理を行いたいが、親要素のクリックイベントも発火してしまう。

2. **パフォーマンスの低下**
   - 深いネスト構造を持つDOMでイベントが伝播すると、不要なイベント処理が実行される可能性があります。

実装した処理を動作確認した際、余計な処理が実行されている場合はバブリングによる影響である可能性を疑ってください。

#### 回避方法
1. **`stopPropagation`の使用**
   - イベントの伝播を停止するには、`stopPropagation()`メソッドを使用します。
   ```javascript
   document.getElementById("child").addEventListener("click", function(event) {
     event.stopPropagation(); // イベントの伝播を停止
     console.log("子要素がクリックされました");
   });
   ```

2. **イベントデリゲーションの活用**
   - 親要素にイベントリスナーを設定し、子要素のイベントを一元管理することで、伝播の影響を最小限に抑えます。
   ```javascript
   document.getElementById("parent").addEventListener("click", function(event) {
     if (event.target.id === "child") {
       console.log("子要素がクリックされました");
     }
   });
   ```

3. **キャプチャリングとバブリングの適切な使い分け**
   - イベントの伝播順序を考慮し、必要に応じてキャプチャリングを有効にすることで、意図した順序でイベントを処理できます。
