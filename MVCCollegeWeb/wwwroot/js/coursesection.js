$(document).ready(function () {

    // Function to bind necessary events
    function bindEvents() {

        $('#filterSession, #filterDepartment, #searchCours').off('change').on('change', function () {
            const selectedSemester = $('#filterSession').val();
            const selectedDepartment = $('#filterDepartment').val();
            const searchedCourse = $('#searchCours').val();

            $.ajax({
                url: '/Home/GetFilteredSemesters',
                method: 'GET',
                data: {
                    semester: selectedSemester,
                    department: selectedDepartment,
                    course: searchedCourse
                },
                success: function (data) {
                    $("#coursesTable").html(data);
                    bindEvents();
                },
                error: function (response) {
                    console.error("Error fetching filtered semesters:", response.responseText);
                    alert("Error fetching filtered semesters: " + response.responseText);
                }
            });
        });

        $('#coursesTable').off('click').on('click', 'tbody tr', function () {
            $('#coursesTable tbody tr').removeClass('selected-row');
            $(this).addClass('selected-row');

            var ScheduleId = $(this).find('th:first').text();

            $.ajax({
                url: '/Home/CoursCombinedView',
                method: 'GET',
                data: {
                    scheduleId: ScheduleId
                },
                success: function (data) {
                    console.log(data);

                    var studentSection = $(data).find("#studentSection");
                    $("#studentTable").html(studentSection);

                    var teacherSection = $(data).find("#teacherSection");
                    $("#teacherView").html(teacherSection);
                    bindEvents();
                },
                error: function (response) {
                    console.error("Error fetching student view:", response.responseText);
                    alert("Error fetching student view: " + response.responseText);
                }
            });
        });

        document.getElementById('changeProfessorButton').addEventListener('click', function () {
            document.getElementById('professorDisplay').style.display = 'none';
            document.getElementById('changeProfessorDropdown').style.display = 'block';
        });

        document.getElementById('confirmProfessorChange').addEventListener('click', function () {
            var selectedProfessor = document.querySelector('#changeProfessorDropdown select').value;
            document.getElementById('professorName').innerText = selectedProfessor;
            document.getElementById('professorDisplay').style.display = 'block';
            document.getElementById('changeProfessorDropdown').style.display = 'none';
        });
    }

    $('#loadCours').click(function () {
        $('.grid-container').load('CoursView', function () {
            bindEvents();
        });
        $('#loadCours').prop('disabled', true);
    });
    $('#loadCours').trigger('click');

    bindEvents();

});
