// Element inside which you want to see the chart.
let element1 = document.querySelector('#gaugeArea1')
let element2 = document.querySelector('#gaugeArea2')
let element3 = document.querySelector('#gaugeArea3')
let element4 = document.querySelector('#gaugeArea4')

let options1 = {
    arcColors: ['var(--chart-color1)', 'var(--border-color)'],
    arcDelimiters: [80],
    rangeLabel: ['0%', '100%'],
    centralLabel: '70%',
}

let options2 = {
    hasNeedle: true,
    needleColor: 'black',
    arcColors: ['var(--chart-color1)', 'var(--chart-color2)', 'var(--chart-color3)'],
    arcDelimiters: [40, 60],
    rangeLabel: ['52', '8'],
    needleStartValue: 50,
}

let options3 = {
    hasNeedle: true,
    outerNeedle: true,
    arcColors: ['var(--border-color)'],
    needleColor: 'var(--chart-color1)',
    arcDelimiters: [],
    rangeLabel: ['-10', '10'],
    centralLabel: '2',
    rangeLabelFontSize: 42,
}

let options5 = {
    hasNeedle: true,
    needleColor: 'black',
    arcColors: [],
    arcDelimiters: [10, 60, 90],
    arcPadding: 6,
    arcPaddingColor: 'var(--card-color)',
    arcColors: ['var(--chart-color1)','var(--chart-color2)','var(--chart-color3)','var(--chart-color4)'],
    arcLabels: ['35', '210', '315'],
    arcLabelFontSize: false,
    rangeLabel: ['0', '350'],
    centralLabel: '175',
    rangeLabelFontSize: false,
    labelsFont: 'Consolas',
}

// Drawing and updating the chart.  
GaugeChart .gaugeChart(element1, 400, options1) .updateNeedle(70)
GaugeChart .gaugeChart(element2, 400, options2) .updateNeedle(20)
GaugeChart .gaugeChart(element3, 400, options3) .updateNeedle(60)
GaugeChart .gaugeChart(element4, 400, options5) .updateNeedle(30)

function edit(id) {
    params = objToUrlStr(id)
    window.open('edit.html?' + params)
}

function objToUrlStr(id) {
    let options = {}
    if (id === 1)
        options = options1
    else if (id === 2)
        options = options2
    else if (id === 3)
        options = options3
    else if (id === 4)
        options = options4
    // stringify Object and delete all spaces from it
    return JSON.stringify(options).replace(/\s/g, '')
}