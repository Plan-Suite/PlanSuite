import Chart from 'chart.js/auto'
import jquery from '../../wwwroot/lib/jquery/dist/jquery';
import { ProjectCommon } from './projectCommon';
import { UrlUtil } from '../UrlUtil';
import { View } from './view';
import { IncompleteTasksDataset, IncompleteTask } from 'models/IncompleteTasksDataset';

(async function () {
    if (!UrlUtil.IsCorrectPage("projects", View.Dashboard)) {
        return;
    }

    var context: HTMLCanvasElement = document.getElementById('incompleteTasks') as HTMLCanvasElement;
    var projectId: number = $("#projectId").val() as number;
    var teamMember: string = $("#filterByTeamMember").val() as string;

    var jsonData = JSON.stringify({ id: projectId, teamMember: teamMember, isCompleted: false });

    $.ajax({
        type: "POST",
        dataType: "json",
        contentType: "application/json",
        url: "/api/Project/GetIncompleteTasksDataset",
        beforeSend: function (request) {
            request.setRequestHeader("RequestVerificationToken", ProjectCommon.GetVerificationToken());
        },
        data: jsonData,
        success: function (success: IncompleteTasksDataset) {
            console.log(`success: ${JSON.stringify(success)}`);

            new Chart(context,
                {
                    type: 'bar',
                    data: {
                        labels: success.incompleteTasks.map(row => row.column),
                        datasets: [
                            {
                                label: 'Incomplete Tasks',
                                data: success.incompleteTasks.map(row => row.count)
                            }
                        ]
                    }
                }
            );
        }
    });

    var jsonData = JSON.stringify({ id: projectId, teamMember: teamMember, isCompleted: true });

    $.ajax({
        type: "POST",
        dataType: "json",
        contentType: "application/json",
        url: "/api/Project/GetIncompleteTasksDataset",
        beforeSend: function (request) {
            request.setRequestHeader("RequestVerificationToken", ProjectCommon.GetVerificationToken());
        },
        data: jsonData,
        success: function (success: IncompleteTasksDataset) {
            console.log(`success: ${JSON.stringify(success)}`);

            new Chart(context,
                {
                    type: 'doughnut',
                    data: {
                        labels: success.incompleteTasks.map(row => row.column),
                        datasets: [
                            {
                                label: 'Completed Tasks',
                                data: success.incompleteTasks.map(row => row.count)
                            }
                        ]
                    }
                }
            );
        }
    });

    /*const data = [
        { year: 2010, count: 10 },
        { year: 2011, count: 20 },
        { year: 2012, count: 15 },
        { year: 2013, count: 25 },
        { year: 2014, count: 22 },
        { year: 2015, count: 30 },
        { year: 2016, count: 28 },
    ];

    new Chart(context,
        {
            type: 'bar',
            data: {
                labels: data.map(row => row.year),
                datasets: [
                    {
                        label: 'Incomplete Tasks',
                        data: data.map(row => row.count)
                    }
                ]
            }
        }
    );*/
})();