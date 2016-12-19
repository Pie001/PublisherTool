function ServerRadioOnClick() {

    var inputs = document.getElementsByName("serverList");

    for (var i = 0; i < inputs.length; i++) {
        if (inputs[i].checked) {
            var path = inputs[i].value.split(",");
            document.getElementById("FromPath").value = path[0];
            document.getElementById("ToPath").value = path[1];
            return;
        }
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
