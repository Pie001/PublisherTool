// バックアップ画面/サーバー区分ラジオボタンクリック時
function ServerRadioOnClick() {

    var inputs = document.getElementsByName("serverList");
    // バックアップ区分プルダウンをクリアする
    var data = '<option selected="selected" value="0">Please select backup</option>';

    for (var i = 0; i < inputs.length; i++) {
        if (inputs[i].checked) {
            var path = inputs[i].value.split(",");
            document.getElementById("FromPath").value = "";
            document.getElementById("ToPath").value = path[1];

            // キャッシュ対応
            var timestamp = new Date().getTime();
            // 選択した商材のパス情報を取得して表示する
            $.getJSON("/Api/BackupSelect/?toPath=" + encodeURIComponent(path[1]) + "&date=" + timestamp, function(json) {

                // 取得した分、プルダウンに表示する。valueは格納パス
                for (var i in json) {
                    data += '<option value="' + json[i].BackupPath + '">' + json[i].viewName + '</option>';
                }
                $("#BackupDivision").empty().append(data);
            });

            return;
        }
    }
}

// バックアップ画面/バックアップ区分プルダウン変更時
function BackupDivisionSelect(selectValue) {
    // 0以外を選択した場合、バックアップフォルダ名を付ける
    if (selectValue != "0") {
        // [Please select backup]以外はvalueにバックアップパスが格納されている
        // 選択したバックアップ区分に当てはまる格納パスを発行元に表示する
        document.getElementById("FromPath").value = selectValue;
    } else {
        // 0を選択した場合、発行元の値をクリアする
        document.getElementById("FromPath").value = "";
    }
}

// 画面読み込み後
// 新規表示＆処理後
window.onload = function() {
    // ローディング画像を非表示にする
    $("#wait").empty();

    // 処理後のリダイレクトなどで、商材区分に0以外の値がある場合、
    // 該当商材のバックアップ一覧を取得して表示する。
    var productSelect = document.getElementById("ProductDivision").value;

    if (productSelect != "0") {
        PublishProductSelect(productSelect);
    }
};
