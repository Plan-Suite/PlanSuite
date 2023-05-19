const path = require('path');

module.exports = {
  entry: path.resolve(__dirname, './scripts/index.ts'),
  module: {
    rules: [
      {
        test: /\.ts?$/,
        use: ['ts-loader'],
        exclude: /node_modules/
      },
    ],
  },
  resolve: {
    extensions: ['.tsx', '.ts', '.js'],
  },
  output: {
    library: {
      name: 'PlanSuite',
      type: 'var'
    },
    filename: 'app-client.js',
    path: path.resolve(__dirname, '../wwwroot/js'),
  }
};