// 発行ボタン押下時
function OnClick() {
    var select = document.getElementById("ProductDivision").value;

    if (select != "0") {
        // 発行中にローディング画像を表示する
        $("#wait").empty().append('<img src="/images/pleasewait.gif">');
        // 発行ボタンを一時見えないようにする
        document.getElementById("pulishButton").style.display = "none";
        // 直前の処理のメッセージをクリア
        $(".validation-summary-errors").empty();
    }
}

// 発行画面/商材区分プルダウン変更時
function PublishProductSelect(select) {
    // 0以外を選択した場合、バックアップ一覧を表示する
    if (select != "0") {

        $("#serverRadioList").empty();
        document.getElementById("FromPath").value = "";
        document.getElementById("ToPath").value = "";

        // キャッシュ対応
        var timestamp = new Date().getTime();
        // 選択した商材のパス情報を取得して表示する
        $.getJSON("/Api/ProductListSelect/?selectValue=" + select + "&date=" + timestamp, function(json) {
            var data = '<label>サーバー区分</label>';
            for (var i in json) {
                var value = json[i].value.toString();
                data += '<div class="serverRadio"><input type="radio" name="serverList" value="' + value + '" onClick="ServerRadioOnClick();">' + json[i].viewName + '</input></div>';
            }
            $("#serverRadioList").empty().append(data);
        });
    } else {
        // 0を選択した場合、発行元＆発行先の値をクリアする
        $("#serverRadioList").empty();
        document.getElementById("FromPath").value = "";
        document.getElementById("ToPath").value = "";
    }
}

