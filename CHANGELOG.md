# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added

## [1.0.5] - 2023-04-04

### Changed

- Fixes a bug where EnumMember attribute enums would have the first letter lowecased

## [1.0.4] - 2023-04-03

### Changed

- Fixes a bug where EnumMember attribute was not taken into account during serialization/deserialization

## [1.0.3] - 2023-03-15

### Changed

- Fixes serialization of DateTime type in the additionalData

## [1.0.2] - 2023-03-10

### Changed

- Bumps abstraction dependency

## [1.0.1] - 2023-03-08

### Changed

- Update minimum version of [`System.Text.Json`](https://www.nuget.org/packages/System.Text.Json) to `6.0.0`.

## [1.0.0] - 2023-02-27

### Added

- GA release

## [1.0.0-rc.3] - 2023-01-27

### Changed

- Relaxed nullability tolerance when merging objects for composed types.

## [1.0.0-rc.2] - 2023-01-17

### Changed

- Adds support for nullable reference types

## [1.0.0-rc.1] - 2022-12-15

### Changed

- Release candidate 1

## [1.0.0-preview.7] - 2022-09-02

### Added

- Added support for composed types serialization.

## [1.0.0-preview.6] - 2022-05-27

### Changed

- Fixes a bug where JsonParseNode.GetChildNode would throw an exception if the property name did not exist in the json.

## [1.0.0-preview.5] - 2022-05-18

### Changed

- Updated abstractions version to 1.0.0.preview8

## [1.0.0-preview.4] - 2022-04-12

### Changed

- Breaking: Changes target runtime to netstandard2.0

## [1.0.0-preview.3] - 2022-04-11

### Changed

- Fixes handling of JsonElement types in additionData during serialization

## [1.0.0-preview.2] - 2022-04-04

### Changed

- Breaking: simplifies the field deserializers.

## [1.0.0-preview.1] - 2022-03-18

### Added

- Initial Nuget release

