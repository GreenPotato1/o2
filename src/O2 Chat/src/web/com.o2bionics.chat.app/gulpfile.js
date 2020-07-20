// ReSharper disable Es6Feature

const gulp = require('gulp');
const rimraf = require('rimraf');
const concat = require('gulp-concat');
const cssmin = require('gulp-cssmin');
const uglify = require('gulp-uglify');
const filter = require('gulp-filter');
const bower = require('main-bower-files');
const merge = require('merge-stream');
const debug = require('gulp-debug');

const paths = {
    webroot: './',
    nodeModules: './node_modules/',
    bowerComponents: './bower_components'
  };
paths.lib = paths.webroot + 'st/lib';


gulp.task(
  'bower:clean',
  function (cb)
  {
    rimraf(paths.lib, cb);
  });

gulp.task(
  'bower:copy',
  //gulp.series(['bower:clean']),
  function ()
  {
    const bowerComponents = gulp
      .src(bower(), { base: paths.bowerComponents })
      .pipe(gulp.dest(paths.lib + '/'));

    return merge(
      pipeSrc(['autolinker/dist/Autolinker*.js'], 'autolinker'),
      pipeSrc(['signalr/jquery.signalR*.js'], 'jquery.signalr'),
      bowerComponents);
  });

gulp.task('default', gulp.series(['bower:copy']));

function pipeSrc(globs, dest)
{
  return gulp
    .src(globs.map(x => paths.nodeModules + x))
    .pipe(gulp.dest(paths.lib + '/' + dest + '/'));
}