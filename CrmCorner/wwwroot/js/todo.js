function addtodo() {
    // Kartın içeriği
    var kartIcerik2 = `
          <label class="task" >
             <input type="checkbox" class="taskCheckbox" id="taskCheckbox" onclick="clickToDoCheckbox(this)">
             <span class="custom-checkbox"></span>
             <input type="text" name="textValue" id="textInput"   placeholder="Yapılacaklar">
           </label>
    `;
    // Kartı kartContainer içine ekle
    $('#kartContainer2').append(kartIcerik2);
}

// Butona tıklandığında kartEkle fonksiyonunu çağır
$('#ekleButton').click(function () {
    addtodo();
});

$("#ekleButton2").keyup(function (event) {
    console.log(event.keyCode);
    if (event.keyCode === 13) {
        console.log(event);
        var inputElement = document.getElementById("ekleButton2");
        var toDoTitleList = inputElement.value;
        $.ajax({
            type: 'POST',
            url: "/ToDoList/ToDoListAddList/",
            encode: true,
            data: { title: toDoTitleList },
            dataType: "json",
            success: function () {
                location.reload();
            }
        });
    }

});


function clickToDoCheckbox(checkbox) {
    var label = checkbox.parentElement.querySelector('input[type="text"]');
    if (checkbox.checked) {
        label.style.textDecoration = "line-through";
    } else {
        label.style.textDecoration = "none";
    }
}
function textVisibleshow() {
    console.log("textVisibleshow girdi");
    var inputElement = document.getElementById("textInput");
    inputElement.style.textDecoration = "none";
}

function textVisible() {
    console.log("textVisible girdi");
    var inputElement = document.getElementById("textInput");
    inputElement.style.textDecoration = "none"; //
}

function save() {
    var taskLabels = document.querySelectorAll('.task');
    var selectedTaskTexts = [];
    var notselectedTaskTexts = [];
    var inputElement = document.getElementById("todotitle");
    var toDoTitle = inputElement.value;
    var maingoal = document.getElementById("maingoal");
    var toDoMainGoal = maingoal.value;

    taskLabels.forEach(function (label) {
        var checkbox = label.querySelector('.taskCheckbox');
        var textInput = label.querySelector('input[type="text"]');
        var maingoal = document.getElementById("maingoal").innerText;
        var values = textInput.value.split(',');
        if (checkbox.checked) {
            selectedTaskTexts.push(textInput.value.trim());
        }
        else {
            notselectedTaskTexts.push(textInput.value.trim());
        }

    });
    var selectedTaskTextsString = selectedTaskTexts.join(",");
    var notselectedTaskTextsString = notselectedTaskTexts.join(",");
    var inputElement = document.getElementById("todolistId");
    var todoListId = inputElement.value;

    $.ajax({
        type: 'POST',
        url: "/ToDoList/ToDoListAdd/",
        encode: true,
        data: { selected: selectedTaskTextsString, unselected: notselectedTaskTextsString, maingoals: toDoMainGoal, title: toDoTitle, itemId: todoListId },
        dataType: "json",
        success: function () {
            document.getElementById("maingoal").value = toDoMainGoal;
            document.getElementById("todotitle").value = toDoTitle;
            var kartIcerik = '';
            var kartIcerik2 = '';
            if (selectedTaskTexts != null) {
                for (var i = 0; i < selectedTaskTexts.length; i++) {
                    var task = selectedTaskTexts[i];
                    kartIcerik += `
                <label class="task">
                    <input type="checkbox" class="taskCheckbox" onclick="clickToDoCheckbox(this)" id="taskCheckbox" @(true ? "checked" : "")>
                    <span class="custom-checkbox"></span>
                    <input type="text" style="text-decoration:line-through" id="textInput" name="textValue" placeholder="Yapılacaklar" value="${task}">
                </label>`;
                }
            }
            if (notselectedTaskTexts != null) {
                for (var i = 0; i < notselectedTaskTexts.length; i++) {
                    var task2 = notselectedTaskTexts[i];
                    kartIcerik2 += `
              <label class="task">
        <input type="checkbox" class="taskCheckbox" onclick="clickToDoCheckbox(this)" id="taskCheckbox" @(false ? "checked" : "")>
        <span class="custom-checkbox"></span>
        <input type="text" name="textValue" id="textInput" placeholder="Yapılacaklar"  value="${task2}">
            </label>`;
                }



            }
            $('#kartContainer2').empty();
            var toplam = kartIcerik + kartIcerik2;
            $('#kartContainer2').append(toplam);

            var input = document.getElementById("textInput");
            var button = document.getElementById("myButton");
            document.getElementById("myModal").style.display = "block";
        }
    });
}
document.querySelector(".close").addEventListener("click", function () {
    // Modalı gizle
    document.getElementById("myModal").style.display = "none";
    location.reload();

});
 



