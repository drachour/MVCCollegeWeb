$(document).ready(function () {

    function fetchFilteredSemesters() {
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
    }

    function fetchCourseCombinedView(scheduleId) {
        $.ajax({
            url: '/Home/CoursCombinedView',
            method: 'GET',
            data: {
                scheduleId: scheduleId
            },
            success: function (data) {
                /*console.log(data);*/

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
    }

    function fetchModalStudent() {
        $.ajax({
            url: '/Home/CallModalStudent',
            method: 'GET',
            success: function (data) {    
                $("#modalStudentDisplay").html(data);
                bindEvents();
                var modalInstance = new bootstrap.Modal(document.getElementById('addStudentModal'));
                modalInstance.show();

            },
            error: function (response) {
                console.error("Error fetching student view:", response.responseText);
                alert("Error fetching student view: " + response.responseText);
            }
        });
    }

    function fetchCourseSemester(teacherId) {

        $.ajax({
            url: '/Home/CourseTable',
            method: 'GET',
            data: {
                teacherId: teacherId
            },
            success: function (data) {             
                $("#coursesTable").html(data);
                bindEvents();
            },
            error: function (response) {
                console.error("Error fetching filtered semesters:", response.responseText);
                alert("Error fetching filtered semesters: " + response.responseText);
            }
        })
    }

    function fetchStudentGrade(studentId, NewGrade, gradeCell)
    {
        $.ajax({
            url: '/Home/UpdateGrade',
            method: 'POST',
            data: {
                studentId: studentId,
                grade: NewGrade
            },
            success: function () {        
                gradeCell.text(NewGrade);
            },
            error: function (jqXHR, textStatus, errorThrown) {
                console.log(jqXHR);
                console.log(textStatus);
                console.log(errorThrown);
                gradeCell.text(NewGrade);
            }
        });
    }

    function fetchStudentTable(studentId)
    {
        $.ajax({
            url: '/Home/InsertCourseStudent',
            method: 'GET',
            data: {
                studentId: studentId
            },
            success: function (data) {
                var studentSection = $(data).find("#studentSection");
                $("#studentTable").html(studentSection);
                bindEvents();
            },
            error: function (response) {
                console.error("Error fetching filtered semesters:", response.responseText);
                alert("Error fetching filtered semesters: " + response.responseText);
            }
        })
    }

    function removeStudentFromCourse(studentId, row) {
        $.ajax({
            url: '/Home/RemoveStudentFromCourse',
            method: 'POST',
            data: {
                studentId: studentId
            },
            success: function () {
                row.remove();
            },
            error: function () {
                alert('Error removing student from course.');
            }
        });
    }

    function toggleCardFooter() {
        if ($('#studentTable tr.selected-row').length === 0) {
            $('.studentView').prop('disabled', true).addClass('disabled');
        } else {
            $('.studentView').prop('disabled', false).removeClass('disabled');
        }
    }

    function fetchStudentInfo(studentData) {
        $.ajax({
            url: '/Home/UpdateStudentInfo',
            method: 'POST',
            data: studentData ,
            success: function (response) {
                if (response.success) {
                    alert('Student information updated successfully.');

                    var selectedRow = $('#studentTable .selected-row');
                    selectedRow.find('th:nth-child(2)').text(studentData.FirstName);
                    selectedRow.find('th:nth-child(3)').text(studentData.LastName);
                    selectedRow.find('th:nth-child(4)').text(studentData.Phone);
                    selectedRow.find('th:nth-child(5)').text(studentData.Email);

                    bindEvents();
                    $('.selected-row').remove('selected-row');
                    $('.card-footer input').val('');
                    $('.card-footer').addClass('disabled');                  
                } else {
                    alert(response.message || 'Error updating student information.');
                }
            },
            error: function () {
                alert('Error updating student information.');
            }
        });
    }

    function filterTable() {
        var prenomFilter = $("#filterPrenom").val().toLowerCase();
        var nomFilter = $("#filterNom").val().toLowerCase();
        var telephoneFilter = $("#filterTelephone").val().toString();
        var courrielFilter = $("#filterCourriel").val().toLowerCase();
        console.log("Filtering for phone number:", telephoneFilter);

        $("#studentTable tbody tr").each(function () {
            var $row = $(this);

            var prenom = $row.data('fname').toLowerCase();
            var nom = $row.data('lname').toLowerCase();
            var telephone = $row.data('phone').toString();
            var courriel = $row.data('mail').toLowerCase();
            console.log("Row phone number:", telephone);
            var isMatch = true;

            if (prenomFilter && !prenom.includes(prenomFilter)) {
                isMatch = false;
            }

            if (nomFilter && !nom.includes(nomFilter)) {
                isMatch = false;
            }

            if (telephoneFilter && !telephone.includes(telephoneFilter)) {
                isMatch = false;
            }

            if (courrielFilter && !courriel.includes(courrielFilter)) {
                isMatch = false;
            }

            if (isMatch) {
                $row.show();
            } else {
                $row.hide();
            }
        });
    }

    function bindEvents() {

        $('#filterSession, #filterDepartment, #searchCours').off('change').on('change', fetchFilteredSemesters);

        $('#coursesTable').off('click', 'tbody tr').on('click', 'tbody tr', function () {
            console.log("Row clicked");
            var selectedId = $(this).find('th:first').text();
            console.log("Selected ID:", selectedId);
            $('#coursesTable tbody tr').removeClass('selected-row');
            $(this).addClass('selected-row');
            fetchCourseCombinedView(selectedId);
        });

        $('#btnModalStudent').off('click').on('click', function (event) {
            if ($('#coursesTable tbody tr.selected-row').length === 0) {
                event.preventDefault();
                alert('Veuillez sélectionner un cours dans le tableau des cours!');
            } else {
                fetchModalStudent();
            }
        });

        $('#changeProfessorButton').off('click').on('click', function () {
            $('#professorDisplay').hide();
            $('#changeProfessorDropdown').show();
        });

        $('#confirmProfessorChange').off('click').on('click', function () {
            var selectedProfessor = $('#changeProfessorDropdown select').val();
            var selectedOption = $('#changeProfessorDropdown select option:selected');
            $('#professorName').text(selectedProfessor);
            $('#professorDisplay').show();
            $('#changeProfessorDropdown').hide();
            var teacherId = selectedOption.attr('data');
            fetchCourseSemester(teacherId);
        });

        $('#studentTable').off('click', '.editable-grade').on('click', '.editable-grade', function () {
            var $this = $(this);
            if ($this.find('input').length) {
                return;
            }
            var grade = $this.text();
            $this.html('<input type="text" class="form-control grade-input" value="' + grade + '">');
            $this.find('input').focus();
        });

        $('#studentTable').off('blur', '.grade-input').on('blur', '.grade-input', function () {
            var $this = $(this);
            var newGrade = $this.val();
            var gradeCell = $this.parent();
            var studentId = $this.closest('tr').find('button[data]').attr('data');
            fetchStudentGrade(studentId, newGrade, gradeCell);
        });

        $('#studentTable').off('keydown', '.grade-input').on('keydown', '.grade-input', function (e) {
            if (e.key === "Enter") {
                $(this).blur();
            }
        });

        $('#btnStudentInsert').off('click').on('click', function () {
            var selectedStudentId = $('#studentDropdown').val();
            if (selectedStudentId !== "") {
                fetchStudentTable(selectedStudentId);
            } else {
                alert("Please select a student.");
            }
            var modalInstance = bootstrap.Modal.getInstance(document.getElementById('addStudentModal'));
            modalInstance.hide();
        });

        $('#studentTable').off('click', '#btnStudentRemove').on('click', '#btnStudentRemove', function () {
            var $this = $(this);
            var studentId = $this.attr('data');
            var $row = $this.closest('tr');

            var confirmation = confirm("Êtes-vous sure de vouloir supprimer cet étudiant du cours?");
            if (confirmation) {
                removeStudentFromCourse(studentId, $row);
            }
        });

        $('#studentTable').off('click', 'tr').on('click', 'tr', function () {
            $('.selected-row').removeClass('selected-row');
            $(this).toggleClass('selected-row');

            $('#prenomInput').val($(this).data('fname'));
            $('#nomInput').val($(this).data('lname'));
            $('#telephoneInput').val($(this).data('phone'));
            $('#courrielInput').val($(this).data('mail'));
            $('#btnStudentSave').data('id', $(this).data('id'));

            toggleCardFooter();
        });

        $('#btnStudentSave').off('click').on('click', function () {
            var studentData = {
                Id: $(this).data('id'),
                FirstName: $('#prenomInput').val(),
                LastName: $('#nomInput').val(),
                Phone: $('#telephoneInput').val(),
                Email: $('#courrielInput').val()
            };

            fetchStudentInfo(studentData);
        });

        $("#filterSection input").on("input", filterTable);

        toggleCardFooter();
    }

    $('#loadEtudiants').off('click').on('click', function () {
        $('.grid-container').load('StudentView', function () {
            bindEvents();
        });
        $(this).prop('disabled', true);
        $('#loadCours').prop('disabled', false);
    })

    $('#loadCours').off('click').on('click', function () {
        $('.grid-container').load('CoursView', function () {
            bindEvents();
        });
        $(this).prop('disabled', true);
        $('#loadEtudiants').prop('disabled', false);
    }).trigger('click');

});
